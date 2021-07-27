using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ConfigService : DiscordBotService
    {
        private Dictionary<Snowflake, SbuGuildConfig> _guildConfigs = new();

        public IReadOnlyDictionary<Snowflake, SbuGuildConfig> GuildConfigs => _guildConfigs;
        public SbuConfiguration GlobalConfiguration { get; }

        public ConfigService(SbuConfiguration configuration) => GlobalConfiguration = configuration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                _guildConfigs = await context.Guilds.ToDictionaryAsync(k => k.Id, v => v.Config, stoppingToken);
            }
        }

        public async Task SetValueAsync(Snowflake guildId, SbuGuildConfig value, bool set = true)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                SbuGuild guild = (await context.GetGuildAsync(guildId))!;
                guild.Config = set ? guild.Config | value : guild.Config & ~value;
                _guildConfigs[guildId] = guild.Config;

                await context.SaveChangesAsync();
            }
        }

        public bool GetValue(Snowflake guildId, SbuGuildConfig value)
            => _guildConfigs.GetValueOrDefault(guildId).HasFlag(value);

        public SbuGuildConfig GetConfig(Snowflake guildId)
            => _guildConfigs.GetValueOrDefault(guildId);
    }
}