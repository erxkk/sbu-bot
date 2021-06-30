using System;
using System.IO;
using System.Linq;
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
        public DbSet<SbuGuild> Guilds { get; set; }
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

        public SbuColorRole AddColorRole(IRole role, Snowflake ownerId)
        {
            SbuColorRole sbuColorRole = new(role, ownerId);
            ColorRoles.Add(sbuColorRole);
            return sbuColorRole;
        }

        public Task<SbuColorRole> GetColorRoleAsync(
            IRole member,
            Func<IQueryable<SbuColorRole>, IQueryable<SbuColorRole>>? query = null,
            CancellationToken cancellationToken = default
        ) => GetColorRoleAsync(member.Id, member.GuildId, query, cancellationToken)!;

        public Task<SbuColorRole?> GetColorRoleAsync(
            Snowflake roleId,
            Snowflake guildId,
            Func<IQueryable<SbuColorRole>, IQueryable<SbuColorRole>>? query = null,
            CancellationToken cancellationToken = default
        ) => (query is { } ? query(ColorRoles) : ColorRoles)
            .FirstOrDefaultAsync(
                m => m.Id == roleId && m.GuildId == guildId,
                _sbuBot?.StoppingToken ?? cancellationToken
            )!;

        public SbuMember AddMember(IMember member)
        {
            SbuMember sbuMember = new(member);
            Members.Add(sbuMember);
            return sbuMember;
        }

        public Task<SbuMember> GetMemberAsync(
            IMember member,
            Func<IQueryable<SbuMember>, IQueryable<SbuMember>>? query = null,
            CancellationToken cancellationToken = default
        ) => GetMemberAsync(member.Id, member.GuildId, query, cancellationToken)!;

        public Task<SbuMember?> GetMemberAsync(
            Snowflake memberId,
            Snowflake guildId,
            Func<IQueryable<SbuMember>, IQueryable<SbuMember>>? query = null,
            CancellationToken cancellationToken = default
        ) => (query is { } ? query(Members) : Members)
            .FirstOrDefaultAsync(
                m => m.Id == memberId && m.GuildId == guildId,
                _sbuBot?.StoppingToken ?? cancellationToken
            )!;

        public SbuGuild AddGuild(IGuild guild)
        {
            SbuGuild sbuGuild = new(guild);
            Guilds.Add(sbuGuild);
            return sbuGuild;
        }

        public Task<SbuGuild> GetGuildAsync(
            IGuild guild,
            Func<IQueryable<SbuGuild>, IQueryable<SbuGuild>>? query = null,
            CancellationToken cancellationToken = default
        ) => GetGuildAsync(guild.Id, query, cancellationToken)!;

        public Task<SbuGuild?> GetGuildAsync(
            Snowflake guildId,
            Func<IQueryable<SbuGuild>, IQueryable<SbuGuild>>? query = null,
            CancellationToken cancellationToken = default
        ) => (query is { } ? query(Guilds) : Guilds).FirstOrDefaultAsync(
            m => m.Id == guildId,
            _sbuBot?.StoppingToken ?? cancellationToken
        )!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(_sbuBot?.StoppingToken ?? cancellationToken);

#region Configuration

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

        internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SbuDbContext>
        {
            public SbuDbContext CreateDbContext(string[] args)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables("DOTNET_")
                    .AddEnvironmentVariables("BOT_")
                    .AddJsonFile("migrations.json")
                    .Build();

                return new(null, new(configuration));
            }
        }

#endregion
    }
}