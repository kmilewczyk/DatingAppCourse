using API.Entities;

namespace API.Extension;

public static class QueryableExtensions
{
    public static IQueryable<Message> MarkUnreadAsRead(this IQueryable<Message> query, string currentUsername)
    {
        var unreadMessages = query.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername);

        foreach (var message in unreadMessages)
        {
            message.DateRead = DateTime.UtcNow;
        }

        return query;
    }
}