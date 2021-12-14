﻿using System.Collections.Immutable;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole,
    IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<Photo> Photos { get; set; }

    public DataContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Photo>().HasQueryFilter(p => p.Approved);

        AddUserRelationships(modelBuilder);
        AddRoleRelationship(modelBuilder);
        AddUserLikeRelationship(modelBuilder);
        AddMessageRelationship(modelBuilder);

        modelBuilder.ApplyUtcDateTimeConverter();
    }

    private static void AddRoleRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();
    }

    private static void AddMessageRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>()
            .HasOne(u => u.Recipient)
            .WithMany(m => m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(u => u.Sender)
            .WithMany(m => m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void AddUserLikeRelationship(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLike>()
            .HasKey(k => new {k.SourceUserId, k.LikedUserId});

        // User liking
        modelBuilder.Entity<UserLike>()
            .HasOne(s => s.SourceUser)
            .WithMany(l => l.LikedUsers)
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Liked user
        modelBuilder.Entity<UserLike>()
            .HasOne(s => s.LikedUser)
            .WithMany(l => l.LikedByUsers)
            .HasForeignKey(s => s.LikedUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void AddUserRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Photos)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .IsRequired(); // Might not be if photos were to independent of users.

        modelBuilder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
}

public static class UtcDateAnnotation
{
    private const String IsUtcAnnotation = "IsUtc";
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

    public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
        builder.HasAnnotation(IsUtcAnnotation, isUtc);

    public static Boolean IsUtc(this IMutableProperty property) =>
        ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

    /// <summary>
    /// Make sure this is called after configuring all your entities.
    /// </summary>
    public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (!property.IsUtc())
                {
                    continue;
                }

                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(UtcConverter);
                }

                if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(UtcNullableConverter);
                }
            }
        }
    }
}