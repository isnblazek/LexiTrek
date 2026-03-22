namespace LexiTrek.Domain.Entities;

public class Dictionary
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SourceLangId { get; set; }
    public int TargetLangId { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public Language SourceLang { get; set; } = null!;
    public Language TargetLang { get; set; } = null!;
    public ICollection<WordGroup> Groups { get; set; } = [];
    public ICollection<DictionaryEntry> Entries { get; set; } = [];
}
