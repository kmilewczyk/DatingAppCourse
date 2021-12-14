namespace API.DTOs.Member;

public class MemberDto : MemberDtoBase
{
    public ICollection<PhotoDto> Photos { get; set; }
}