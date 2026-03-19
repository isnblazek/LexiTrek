using Microsoft.AspNetCore.Identity;

namespace LexiTrek.Domain.Entities;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<WordGroup> OwnedGroups { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<GroupSubscription> Subscriptions { get; set; } = [];
    public ICollection<TrainingSession> TrainingSessions { get; set; } = [];
    public ICollection<WordProgress> WordProgresses { get; set; } = [];
}
