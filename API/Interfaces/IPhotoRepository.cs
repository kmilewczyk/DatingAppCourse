using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<Photo?> GetPhoto(int id);
    Task<Photo?> GetPhotoWithUser(int id);
    Task ApprovePhotoAsync(Photo photo);
    Task RejectPhotoAsync(Photo photo);
    Task<List<PhotoForModerationDto>> GetUnapprovedPhotosAsync();
}