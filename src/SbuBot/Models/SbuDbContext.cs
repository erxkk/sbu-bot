using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

using SbuBot.Extensions;

namespace SbuBot.Models
{
    public sealed class SbuDbContext : DbContext
    {
        private readonly SbuBot? _sbuBot;
        private readonly SbuBotConfiguration _configuration;

#nullable disable
        public DbSet<SbuMember> Members { get; set; }
        public DbSet<SbuColorRole> ColorRoles { get; set; }
        public DbSet<SbuTag> Tags { get; set; }
        public DbSet<SbuReminder> Reminders { get; set; }
#nullable enable

        public SbuDbContext(SbuBot? sbuBot, SbuBotConfiguration configuration)
        {
            _sbuBot = sbuBot;
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_configuration.DbConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var snowflakeConverter = new ValueConverter<Snowflake, ulong>(
                static snowflake => snowflake.RawValue,
                static @ulong => new(@ulong)
            );

            var colorConverter = new ValueConverter<Color, int>(
                static color => color.RawValue,
                static @int => new(@int)
            );

            var datetimeConverter = new ValueConverter<DateTimeOffset, long>(
                static datetime => datetime.ToUnixTimeMilliseconds(),
                static @long => DateTimeOffset.FromUnixTimeMilliseconds(@long)
            );

            modelBuilder.UseValueConverterForType(snowflakeConverter);
            modelBuilder.UseValueConverterForType(colorConverter);
            modelBuilder.UseValueConverterForType(datetimeConverter);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SbuDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(_sbuBot?.StoppingToken ?? cancellationToken);

        internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SbuDbContext>
        {
            public SbuDbContext CreateDbContext(string[] args)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables("DOTNET_")
                    .AddYamlFile("config.yaml")
                    .AddCommandLine(args)
                    .Build();

                return new(null, new(configuration));
            }
        }
    }
}