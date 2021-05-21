using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SbuBot.Extensions
{
    // stolen from Zackattack01 :^)
    // improved to include nullable for value types and use generics for compile time safety
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModuleBuilderExtensions
    {
        public static ModelBuilder UseValueConverterForType<TClr, TDb>(
            this ModelBuilder modelBuilder,
            ValueConverter<TClr, TDb> converter
        ) where TClr : class
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (PropertyInfo property in entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(TClr)))
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(converter);
                }
            }

            return modelBuilder;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModuleBuilderValueExtensions
    {
        public static ModelBuilder UseValueConverterForType<TClr, TDb>(
            this ModelBuilder modelBuilder,
            ValueConverter<TClr, TDb> converter
        ) where TClr : struct
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (PropertyInfo property in entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(TClr) || p.PropertyType == typeof(TClr?)))
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(converter);
                }
            }

            return modelBuilder;
        }
    }
}