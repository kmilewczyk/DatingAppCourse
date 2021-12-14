namespace API.DTOs.Member;

public class ProfileMemberDto : MemberDtoBase
{
    public ICollection<EditPhotoDto> Photos { get; set; }
}