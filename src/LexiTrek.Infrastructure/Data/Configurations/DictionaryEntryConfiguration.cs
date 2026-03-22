using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class DictionaryEntryConfiguration : IEntityTypeConfiguration<DictionaryEntry>
{
    public void Configure(EntityTypeBuilder<DictionaryEntry> builder)
    {
        builder.HasKey(e => new { e.Id, e.DictionaryId });
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.Property(e => e.GroupIds)
            .HasColumnType("bigint[]")
            .HasDefaultValueSql("'{}'");
        builder.HasIndex(e => e.GroupIds).HasMethod("gin");

        builder.HasOne(e => e.Dictionary)
            .WithMany(d => d.Entries)
            .HasForeignKey(e => e.DictionaryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WordPair)
            .WithMany()
            .HasForeignKey(e => e.WordPairId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.DictionaryId, e.WordPairId }).IsUnique();
        builder.HasIndex(e => e.WordPairId);
    }
}
