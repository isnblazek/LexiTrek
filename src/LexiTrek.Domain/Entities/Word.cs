namespace LexiTrek.Domain.Entities;

public class Word
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public WordGroup Group { get; set; } = null!;
    public ICollection<WordTag> WordTags { get; set; } = [];
}
