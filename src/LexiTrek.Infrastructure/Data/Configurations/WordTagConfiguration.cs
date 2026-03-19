using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class WordTagConfiguration : IEntityTypeConfiguration<WordTag>
{
    public void Configure(EntityTypeBuilder<WordTag> builder)
    {
        builder.HasKey(wt => new { wt.WordId, wt.TagId });

        builder.HasOne(wt => wt.Word)
            .WithMany(w => w.WordTags)
            .HasForeignKey(wt => wt.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wt => wt.Tag)
            .WithMany(t => t.WordTags)
            .HasForeignKey(wt => wt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
