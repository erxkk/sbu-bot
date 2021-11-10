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
        private readonly SbuBot _sbuBot;
        private readonly SbuConfiguration _configuration;

#nullable disable
        public DbSet<SbuGuild> Guilds { get; set; }
        public DbSet<SbuMember> Members { get; set; }
        public DbSet<SbuColorRole> ColorRoles { get; set; }
        public DbSet<SbuTag> Tags { get; set; }
        public DbSet<SbuAutoResponse> AutoResponses { get; set; }
        public DbSet<SbuReminder> Reminders { get; set; }
        public DbSet<SbuRole> Roles { get; set; }
#nullable enable

        public SbuDbContext(SbuBot sbuBot, SbuConfiguration configuration)
        {
            _sbuBot = sbuBot;
            _configuration = configuration;
        }

        // role
        public SbuRole AddRole(IRole role, string? description = null)
            => Roles.Add(new(role, description)).Entity;

        public Task<SbuRole?> GetRoleFullAsync(IRole role)
            => GetRoleAsync(role, q => q.Include(r => r.Guild));

        public Task<SbuRole?> GetRoleAsync(
            IRole role,
            Func<IQueryable<SbuRole>, IQueryable<SbuRole>>? query = null
        ) => GetRoleAsync(role.Id, role.GuildId, query);

        public Task<SbuRole?> GetRoleAsync(
            Snowflake roleId,
            Snowflake guildId,
            Func<IQueryable<SbuRole>, IQueryable<SbuRole>>? query = null
        ) => (query is { } ? query(Roles) : Roles).FirstOrDefaultAsync(
            m => m.Id == roleId && m.GuildId == guildId,
            _sbuBot.StoppingToken
        )!;

        // color role
        public SbuColorRole AddColorRole(IRole role, Snowflake ownerId)
            => ColorRoles.Add(new(role, ownerId)).Entity;

        public Task<SbuColorRole?> GetColorRoleFullAsync(IRole role)
            => GetColorRoleAsync(role, q => q.Include(r => r.Guild).Include(r => r.Owner));

        public Task<SbuColorRole?> GetColorRoleAsync(
            IRole role,
            Func<IQueryable<SbuColorRole>, IQueryable<SbuColorRole>>? query = null
        ) => GetColorRoleAsync(role.Id, role.GuildId, query);

        public Task<SbuColorRole?> GetColorRoleAsync(
            Snowflake roleId,
            Snowflake guildId,
            Func<IQueryable<SbuColorRole>, IQueryable<SbuColorRole>>? query = null
        ) => (query is { } ? query(ColorRoles) : ColorRoles).FirstOrDefaultAsync(
            m => m.Id == roleId && m.GuildId == guildId,
            _sbuBot.StoppingToken
        )!;

        // member
        public SbuMember AddMember(IMember member)
        {
            SbuMember sbuMember = new(member);
            Members.Add(sbuMember);
            return sbuMember;
        }

        public Task<SbuMember?> GetMemberFullAsync(IMember member)
            => GetMemberAsync(member, q => q.Include(m => m.Guild).Include(m => m.ColorRole));

        public Task<SbuMember?> GetMemberAsync(
            IMember member,
            Func<IQueryable<SbuMember>, IQueryable<SbuMember>>? query = null
        ) => GetMemberAsync(member.Id, member.GuildId, query);

        public Task<SbuMember?> GetMemberAsync(
            Snowflake memberId,
            Snowflake guildId,
            Func<IQueryable<SbuMember>, IQueryable<SbuMember>>? query = null
        ) => (query is { } ? query(Members) : Members).FirstOrDefaultAsync(
            m => m.Id == memberId && m.GuildId == guildId,
            _sbuBot.StoppingToken
        )!;

        // guild
        public SbuGuild AddGuild(IGuild guild)
            => Guilds.Add(new(guild)).Entity;

        public Task<SbuGuild?> GetGuildAsync(
            IGuild guild,
            Func<IQueryable<SbuGuild>, IQueryable<SbuGuild>>? query = null
        ) => GetGuildAsync(guild.Id, query);

        public Task<SbuGuild?> GetGuildAsync(
            Snowflake guildId,
            Func<IQueryable<SbuGuild>, IQueryable<SbuGuild>>? query = null
        ) => (query is { } ? query(Guilds) : Guilds).FirstOrDefaultAsync(
            m => m.Id == guildId,
            _sbuBot.StoppingToken
        )!;

        // tag
        public SbuTag AddTag(Snowflake ownerId, Snowflake guildId, string name, string content)
            => Tags.Add(new(ownerId, guildId, name, content)).Entity;

        public Task<SbuTag?> GetTagFullAsync(
            string name,
            Snowflake guildId
        ) => GetTagAsync(name, guildId, q => q.Include(t => t.Guild).Include(t => t.Owner));

        public Task<SbuTag?> GetTagAsync(
            string name,
            Snowflake guildId,
            Func<IQueryable<SbuTag>, IQueryable<SbuTag>>? query = null
        ) => (query is { } ? query(Tags) : Tags).FirstOrDefaultAsync(
            t => t.Name == name && t.GuildId == guildId,
            _sbuBot.StoppingToken
        )!;

        // auto response
        public SbuAutoResponse AddAutoResponse(Snowflake guildId, string trigger, string response)
            => AutoResponses.Add(new(guildId, trigger, response)).Entity;

        public Task<SbuAutoResponse?> GetAutoResponseFullAsync(string trigger, Snowflake guildId)
            => GetAutoResponseAsync(trigger, guildId, q => q.Include(ar => ar.Guild));

        public Task<SbuAutoResponse?> GetAutoResponseAsync(
            string trigger,
            Snowflake guildId,
            Func<IQueryable<SbuAutoResponse>, IQueryable<SbuAutoResponse>>? query = null
        ) => (query is { } ? query(AutoResponses) : AutoResponses).FirstOrDefaultAsync(
            ar => ar.Trigger == trigger && ar.GuildId == guildId,
            _sbuBot.StoppingToken
        )!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(_sbuBot.StoppingToken);

#region Configuration

        // TODO: use Table-Per-Type HasBaseType on EntityTypeConfiguration once supported on ef core to reduce type config duplication
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

        public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SbuDbContext>
        {
            public SbuDbContext CreateDbContext(string[] args)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables("DOTNET_")
                    .AddEnvironmentVariables("BOT_")
                    .Build();

                return new(null!, new(configuration));
            }
        }

#endregion
    }
}