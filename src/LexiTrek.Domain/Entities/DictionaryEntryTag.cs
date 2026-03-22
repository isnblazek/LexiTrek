namespace LexiTrek.Domain.Entities;

public class DictionaryEntryTag
{
    public long DictionaryEntryId { get; set; }
    public long DictionaryId { get; set; }
    public long TagId { get; set; }

    public DictionaryEntry Entry { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
