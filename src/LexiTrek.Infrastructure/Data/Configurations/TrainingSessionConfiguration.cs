using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.User)
            .WithMany(u => u.TrainingSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Group)
            .WithMany()
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Tag)
            .WithMany()
            .HasForeignKey(s => s.TagId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ClientSessionId);
    }
}
