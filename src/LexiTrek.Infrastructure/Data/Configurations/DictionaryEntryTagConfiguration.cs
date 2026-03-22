using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class DictionaryEntryTagConfiguration : IEntityTypeConfiguration<DictionaryEntryTag>
{
    public void Configure(EntityTypeBuilder<DictionaryEntryTag> builder)
    {
        builder.HasKey(et => new { et.DictionaryEntryId, et.DictionaryId, et.TagId });

        builder.HasOne(et => et.Entry)
            .WithMany(e => e.Tags)
            .HasForeignKey(et => new { et.DictionaryEntryId, et.DictionaryId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(et => et.Tag)
            .WithMany(t => t.EntryTags)
            .HasForeignKey(et => et.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
