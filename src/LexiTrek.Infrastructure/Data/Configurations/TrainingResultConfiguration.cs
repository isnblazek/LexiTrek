using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class TrainingResultConfiguration : IEntityTypeConfiguration<TrainingResult>
{
    public void Configure(EntityTypeBuilder<TrainingResult> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseIdentityAlwaysColumn();

        builder.HasOne(r => r.Session)
            .WithMany(s => s.Results)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.WordPair)
            .WithMany()
            .HasForeignKey(r => r.WordPairId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.SessionId);
    }
}
