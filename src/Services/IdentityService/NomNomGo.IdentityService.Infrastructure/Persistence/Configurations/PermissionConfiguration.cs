using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomNomGo.IdentityService.Domain.Entities;
using NomNomGo.IdentityService.Infrastructure.Persistence.Configurations.Base;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Configurations
{
    public class PermissionConfiguration : BaseEntityConfiguration<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> builder)
        {
            base.Configure(builder);

            builder.ToTable("Permissions");

            // Установка свойств
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Установка индексов
            builder.HasIndex(p => p.Name).IsUnique();

            // Установка отношений
            builder.HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}