namespace LexiTrek.Domain.Entities;

public class UserWordProgress
{
    public string UserId { get; set; } = string.Empty;
    public long WordPairId { get; set; }
    public double EaseFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; }
    public int Repetitions { get; set; }
    public DateOnly NextReview { get; set; }
    public DateTime? LastReviewedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public WordPair WordPair { get; set; } = null!;
}
