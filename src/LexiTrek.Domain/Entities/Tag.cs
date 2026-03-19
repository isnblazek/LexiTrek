namespace LexiTrek.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public AppUser Owner { get; set; } = null!;
    public ICollection<WordTag> WordTags { get; set; } = [];
}
