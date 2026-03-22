using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class DictionaryConfiguration : IEntityTypeConfiguration<Dictionary>
{
    public void Configure(EntityTypeBuilder<Dictionary> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).UseIdentityAlwaysColumn();

        builder.HasOne(d => d.User)
            .WithMany(u => u.Dictionaries)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.SourceLang)
            .WithMany()
            .HasForeignKey(d => d.SourceLangId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.TargetLang)
            .WithMany()
            .HasForeignKey(d => d.TargetLangId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.UserId);
    }
}
