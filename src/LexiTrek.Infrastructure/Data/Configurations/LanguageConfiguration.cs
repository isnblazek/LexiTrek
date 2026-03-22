using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Code).HasMaxLength(10).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Language { Id = 1, Code = "cs", Name = "Čeština" },
            new Language { Id = 2, Code = "en", Name = "Angličtina" },
            new Language { Id = 3, Code = "de", Name = "Němčina" },
            new Language { Id = 4, Code = "fr", Name = "Francouzština" },
            new Language { Id = 5, Code = "es", Name = "Španělština" },
            new Language { Id = 6, Code = "it", Name = "Italština" },
            new Language { Id = 7, Code = "pl", Name = "Polština" },
            new Language { Id = 8, Code = "sk", Name = "Slovenština" },
            new Language { Id = 9, Code = "ru", Name = "Ruština" },
            new Language { Id = 10, Code = "pt", Name = "Portugalština" },
            new Language { Id = 11, Code = "nl", Name = "Holandština" },
            new Language { Id = 12, Code = "uk", Name = "Ukrajinština" }
        );
    }
}
