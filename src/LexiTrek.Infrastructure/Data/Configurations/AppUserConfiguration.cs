using LexiTrek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiTrek.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(200);

        // Shadow properties for refresh tokens
        builder.Property<string>("RefreshToken").HasMaxLength(200).IsRequired(false);
        builder.Property<DateTime?>("RefreshTokenExpiry").IsRequired(false);
    }
}
