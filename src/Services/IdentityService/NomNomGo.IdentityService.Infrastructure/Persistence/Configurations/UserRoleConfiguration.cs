using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomNomGo.IdentityService.Domain.Entities.Relationships;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            // Установка составного первичного ключа
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            // Установка отношений
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Установка индексов
            builder.HasIndex(ur => ur.UserId);
            builder.HasIndex(ur => ur.RoleId);
        }
    }
}