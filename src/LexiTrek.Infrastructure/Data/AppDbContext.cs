using LexiTrek.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<WordPair> WordPairs => Set<WordPair>();
    public DbSet<Dictionary> Dictionaries => Set<Dictionary>();
    public DbSet<WordGroup> WordGroups => Set<WordGroup>();
public DbSet<DictionaryEntry> DictionaryEntries => Set<DictionaryEntry>();
    public DbSet<DictionaryEntryTag> DictionaryEntryTags => Set<DictionaryEntryTag>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingResult> TrainingResults => Set<TrainingResult>();
    public DbSet<UserWordProgress> UserWordProgresses => Set<UserWordProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
