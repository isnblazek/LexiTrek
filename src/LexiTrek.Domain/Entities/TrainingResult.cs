using LexiTrek.Domain.Enums;

namespace LexiTrek.Domain.Entities;

public class TrainingResult
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid WordId { get; set; }
    public TrainingResultType Result { get; set; }
    public DateTime AnsweredAt { get; set; }

    public TrainingSession Session { get; set; } = null!;
    public Word Word { get; set; } = null!;
}
