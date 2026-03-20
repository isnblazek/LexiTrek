using LexiTrek.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Dictionary> Dictionaries => Set<Dictionary>();
    public DbSet<WordGroup> WordGroups => Set<WordGroup>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<WordTag> WordTags => Set<WordTag>();
    public DbSet<GroupSubscription> GroupSubscriptions => Set<GroupSubscription>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingResult> TrainingResults => Set<TrainingResult>();
    public DbSet<WordProgress> WordProgresses => Set<WordProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
