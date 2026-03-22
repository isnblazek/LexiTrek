namespace LexiTrek.Domain.Entities;

public class WordPair
{
    public long Id { get; set; }
    public long SourceWordId { get; set; }
    public long TargetWordId { get; set; }

    public Word SourceWord { get; set; } = null!;
    public Word TargetWord { get; set; } = null!;
}
