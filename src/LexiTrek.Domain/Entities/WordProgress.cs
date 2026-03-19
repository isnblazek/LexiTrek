namespace LexiTrek.Domain.Entities;

public class WordProgress
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid WordId { get; set; }
    public double EaseFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; }
    public int RepetitionCount { get; set; }
    public DateOnly NextReviewDate { get; set; }
    public DateTime? LastReviewedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public Word Word { get; set; } = null!;
}
