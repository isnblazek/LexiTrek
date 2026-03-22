namespace LexiTrek.Domain.Entities;

public class DictionaryEntry
{
    public long Id { get; set; }
    public long DictionaryId { get; set; }
    public long WordPairId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public long[] GroupIds { get; set; } = [];

    public Dictionary Dictionary { get; set; } = null!;
    public WordPair WordPair { get; set; } = null!;
    public ICollection<DictionaryEntryTag> Tags { get; set; } = [];
}
