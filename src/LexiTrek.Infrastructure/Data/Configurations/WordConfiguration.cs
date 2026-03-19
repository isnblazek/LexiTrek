using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Czech).HasMaxLength(500).IsRequired();
        builder.Property(w => w.English).HasMaxLength(500).IsRequired();
        builder.Property(w => w.Notes).HasMaxLength(1000);

        builder.HasOne(w => w.Group)
            .WithMany(g => g.Words)
            .HasForeignKey(w => w.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.GroupId);
    }
}
