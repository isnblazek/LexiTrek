namespace LexiTrek.Domain.Entities;

public class Word
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;
}
