using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace API.Extension;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        
        services.AddScoped<ITokenService, TokenService>();
        
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
    
    public static IServiceCollection AddApplicationSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebAPIv5", Version = "v1"});
            c.AddSecurityDefinition("jwtAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Scheme = "Bearer",
                Description = "JWT"
            });

            c.OperationFilter<SecurityRequirementsOperationFilter>(true, "jwtAuth");
        });
        
        return services;
    }
}