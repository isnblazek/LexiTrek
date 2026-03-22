using LexiTrek.Domain.Enums;

namespace LexiTrek.Domain.Entities;

public class TrainingResult
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public long WordPairId { get; set; }
    public TrainingResultType Result { get; set; }
    public DateTime AnsweredAt { get; set; }

    public TrainingSession Session { get; set; } = null!;
    public WordPair WordPair { get; set; } = null!;
}
