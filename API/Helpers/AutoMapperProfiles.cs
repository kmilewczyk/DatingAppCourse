using API.Data;
using API.DTOs;
using API.DTOs.Member;
using API.Entities;
using API.Extension;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                options => options.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url)
            )
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
        
        CreateMap<AppUser, ProfileMemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                options => options.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url)
            )
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

        CreateMap<Photo, PhotoDto>();
        CreateMap<Photo, EditPhotoDto>();
        CreateMap<Photo, PhotoForModerationDto>()
            .ForMember(dest => dest.Username, options => options.MapFrom(source => source.User.UserName));
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SenderPhotoUrl, options => options.MapFrom(
                source => source.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.RecipientPhotoUrl, options => options.MapFrom(
                source => source.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
    }
}