using LexiTrek.Domain.Enums;

namespace LexiTrek.Domain.Entities;

public class TrainingSession
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TrainingMode Mode { get; set; }
    public long? GroupId { get; set; }
    public long? TagId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public Guid ClientSessionId { get; set; }

    public AppUser User { get; set; } = null!;
    public WordGroup? Group { get; set; }
    public Tag? Tag { get; set; }
    public ICollection<TrainingResult> Results { get; set; } = [];
}
