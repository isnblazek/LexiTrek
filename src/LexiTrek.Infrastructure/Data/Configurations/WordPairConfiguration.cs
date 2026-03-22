using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class WordPairConfiguration : IEntityTypeConfiguration<WordPair>
{
    public void Configure(EntityTypeBuilder<WordPair> builder)
    {
        builder.HasKey(wp => wp.Id);
        builder.Property(wp => wp.Id).UseIdentityAlwaysColumn();

        builder.HasOne(wp => wp.SourceWord)
            .WithMany()
            .HasForeignKey(wp => wp.SourceWordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wp => wp.TargetWord)
            .WithMany()
            .HasForeignKey(wp => wp.TargetWordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wp => new { wp.SourceWordId, wp.TargetWordId }).IsUnique();
    }
}
