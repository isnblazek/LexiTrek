namespace LexiTrek.Domain.Entities;

public class WordTag
{
    public Guid WordId { get; set; }
    public Guid TagId { get; set; }

    public Word Word { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
