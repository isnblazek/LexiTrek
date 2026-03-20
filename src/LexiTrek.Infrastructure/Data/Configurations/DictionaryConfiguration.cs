using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class DictionaryConfiguration : IEntityTypeConfiguration<Dictionary>
{
    public void Configure(EntityTypeBuilder<Dictionary> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.SourceLanguage).HasMaxLength(100).IsRequired();
        builder.Property(d => d.TargetLanguage).HasMaxLength(100).IsRequired();

        builder.HasOne(d => d.Owner)
            .WithMany(u => u.Dictionaries)
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.OwnerId);
    }
}
