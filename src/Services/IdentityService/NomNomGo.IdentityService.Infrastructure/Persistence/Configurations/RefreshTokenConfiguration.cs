using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Infrastructure.Persistence.Configurations.Base;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
    {
        public override void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            base.Configure(builder);

            builder.ToTable("RefreshTokens");

            // Установка свойств
            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");

            // Установка индексов
            builder.HasIndex(rt => rt.Token).IsUnique();
            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.ExpiresAt);

            // Установка отношений
            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}