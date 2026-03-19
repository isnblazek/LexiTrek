using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class TrainingResultConfiguration : IEntityTypeConfiguration<TrainingResult>
{
    public void Configure(EntityTypeBuilder<TrainingResult> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Session)
            .WithMany(s => s.Results)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Word)
            .WithMany()
            .HasForeignKey(r => r.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.SessionId);
    }
}
