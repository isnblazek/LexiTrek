using Microsoft.AspNetCore.Identity;

namespace LexiTrek.Domain.Entities;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Dictionary> Dictionaries { get; set; } = [];
    public ICollection<WordGroup> OwnedGroups { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<TrainingSession> TrainingSessions { get; set; } = [];
    public ICollection<UserWordProgress> UserWordProgresses { get; set; } = [];
}
