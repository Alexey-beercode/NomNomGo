using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NomNomGo.IdentityService.Domain.Common;

namespace NomNomGo.IdentityService.Infrastructure.Persistence.Configurations.Base
{
    /// <summary>
    /// Базовая конфигурация Fluent API для сущностей
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            // Установка первичного ключа
            builder.HasKey(e => e.Id);
            
            // Установка автогенерации GUID (используется в PostgreSQL)
            builder.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
        }
    }
}