namespace LexiTrek.Domain.Entities;

public class GroupSubscription
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public DateTime SubscribedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public WordGroup Group { get; set; } = null!;
}
