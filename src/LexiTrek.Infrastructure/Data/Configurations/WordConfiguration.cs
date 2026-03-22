using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).UseIdentityAlwaysColumn();
        builder.Property(w => w.Text).HasMaxLength(500).IsRequired();

        builder.HasOne(w => w.Language)
            .WithMany()
            .HasForeignKey(w => w.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.Text, w.LanguageId }).IsUnique();
    }
}
