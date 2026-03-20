using LexiTrek.Domain.Enums;

namespace LexiTrek.Domain.Entities;

public class Dictionary
{
    public Guid Id { get; set; }
    public Language SourceLanguage { get; set; }
    public Language TargetLanguage { get; set; }
    public string? OwnerId { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
    public DateTime CreatedAt { get; set; }

    public AppUser? Owner { get; set; }
    public ICollection<WordGroup> WordGroups { get; set; } = [];
}
