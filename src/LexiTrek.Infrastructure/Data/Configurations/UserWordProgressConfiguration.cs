using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class UserWordProgressConfiguration : IEntityTypeConfiguration<UserWordProgress>
{
    public void Configure(EntityTypeBuilder<UserWordProgress> builder)
    {
        // Composite PK — partition key (UserId) je součástí PK
        builder.HasKey(p => new { p.UserId, p.WordPairId });

        builder.Property(p => p.EaseFactor).HasDefaultValue(2.5);
        builder.Property(p => p.IntervalDays).HasDefaultValue(1);
        builder.Property(p => p.Repetitions).HasDefaultValue(0);
        builder.Property(p => p.TotalReviews).HasDefaultValue(0);
        builder.Property(p => p.CorrectCount).HasDefaultValue(0);
        builder.Property(p => p.IncorrectCount).HasDefaultValue(0);

        builder.HasOne(p => p.User)
            .WithMany(u => u.UserWordProgresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.WordPair)
            .WithMany()
            .HasForeignKey(p => p.WordPairId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => new { p.UserId, p.NextReview });
    }
}
