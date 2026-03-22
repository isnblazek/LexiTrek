using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class WordGroupConfiguration : IEntityTypeConfiguration<WordGroup>
{
    public void Configure(EntityTypeBuilder<WordGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).UseIdentityAlwaysColumn();
        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(1000);

        builder.HasOne(g => g.Owner)
            .WithMany(u => u.OwnedGroups)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Dictionary)
            .WithMany(d => d.Groups)
            .HasForeignKey(g => g.DictionaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.SourceGroup)
            .WithMany()
            .HasForeignKey(g => g.SourceGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(g => g.OwnerId);
        builder.HasIndex(g => g.DictionaryId);
        builder.HasIndex(g => g.IsPublic);
    }
}
