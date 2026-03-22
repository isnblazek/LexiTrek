namespace LexiTrek.Domain.Entities;

public class WordGroup
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public long DictionaryId { get; set; }
    public bool IsPublic { get; set; }
    public long? SourceGroupId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AppUser Owner { get; set; } = null!;
    public Dictionary Dictionary { get; set; } = null!;
    public WordGroup? SourceGroup { get; set; }
}
