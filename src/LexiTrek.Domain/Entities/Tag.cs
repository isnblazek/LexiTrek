namespace LexiTrek.Domain.Entities;

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public AppUser Owner { get; set; } = null!;
    public ICollection<DictionaryEntryTag> EntryTags { get; set; } = [];
}
