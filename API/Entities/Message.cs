namespace API.Entities;

public class Message
{
    private DateTime? _dateRead;
    private DateTime _messageSent = DateTime.UtcNow;
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; }
    public AppUser Sender { get; set; }
    public int RecipientId { get; set; }
    public string RecipientUsername { get; set; }
    public AppUser Recipient { get; set; }
    public string Content { get; set; }
    public bool SenderDeleted { get; set; }
    public bool RecipientDeleted { get; set; }

    public DateTime? DateRead
    {
        get => _dateRead;
        set => _dateRead = value?.ToUniversalTime();
    }

    public DateTime MessageSent
    {
        get => _messageSent;
        set => _messageSent = value.ToUniversalTime();
    }
}