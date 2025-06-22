using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Infrastructure.Persistence.Configurations.Base;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : BaseEntityConfiguration<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);

            builder.ToTable("Roles");

            // Установка свойств
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Установка индексов
            builder.HasIndex(r => r.Name).IsUnique();

            // Установка отношений
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}