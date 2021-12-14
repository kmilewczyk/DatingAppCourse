#nullable enable
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public PhotoRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task ApprovePhotoAsync(Photo photo)
    {
        photo.Approved = true;
        return Task.CompletedTask;
    }

    public async Task<Photo?> GetPhoto(int id)
    {
        return await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Photo?> GetPhotoWithUser(int id)
    {
        return await _context.Photos.IgnoreQueryFilters().Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task RejectPhotoAsync(Photo photo)
    {
        _context.Photos.Remove(photo);
        return Task.CompletedTask;
    }

    public async Task<List<PhotoForModerationDto>> GetUnapprovedPhotosAsync()
    {
        return await _context.Photos.IgnoreQueryFilters().Include(photo => photo.User)
            .Where(photo => !photo.Approved)
            .ProjectTo<PhotoForModerationDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
}