using LexiTrek.Domain.Enums;

namespace LexiTrek.Domain.Entities;

public class WordGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public Visibility Visibility { get; set; } = Visibility.Private;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AppUser Owner { get; set; } = null!;
    public ICollection<Word> Words { get; set; } = [];
    public ICollection<GroupSubscription> Subscribers { get; set; } = [];
}
