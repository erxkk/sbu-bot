using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ChatService : DiscordBotService
    {
        private readonly ConfigService _configService;
        private readonly ConcurrentDictionary<(Snowflake GuildId, string Trigger), string> _autoResponses = new();

        public IReadOnlyDictionary<(Snowflake GuildId, string Trigger), string> AutoResponses => _autoResponses;
        public override int Priority => int.MaxValue - 1;

        public ChatService(ConfigService configService)
            => _configService = configService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<SbuAutoResponse> autoResponses;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                autoResponses = await context.AutoResponses.ToListAsync(stoppingToken);
            }

            foreach (SbuAutoResponse autoResponse in autoResponses)
                _autoResponses[(autoResponse.GuildId, autoResponse.Trigger)] = autoResponse.Response;
        }

        public async Task SetAutoResponseAsync(Snowflake guildId, string trigger, string response)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                _ = context.AddAutoResponse(guildId, trigger, response);
                _autoResponses[(guildId, trigger)] = response;

                await context.SaveChangesAsync();
            }
        }

        public IReadOnlyDictionary<string, string> GetAutoResponses(Snowflake guildId)
            => _autoResponses.Where(k => k.Key.GuildId == guildId).ToDictionary(k => k.Key.Trigger, v => v.Value);

        public string? GetAutoResponse(Snowflake guildId, string trigger)
            => _autoResponses.GetValueOrDefault((guildId, trigger));

        public async Task RemoveAutoResponseAsync(Snowflake guildId, string trigger)
        {
            if (_autoResponses.TryRemove((guildId, trigger), out string? response))
            {
                using (IServiceScope scope = Bot.Services.CreateScope())
                {
                    SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                    context.AutoResponses.Remove(new(guildId, trigger, response));
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveAutoResponsesAsync(Snowflake guildId)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                List<SbuAutoResponse> autoResponses = await context.AutoResponses
                    .Where(ar => ar.GuildId == guildId)
                    .ToListAsync();

                foreach (SbuAutoResponse sbuAutoResponse in autoResponses)
                    _autoResponses.TryRemove((sbuAutoResponse.GuildId, sbuAutoResponse.Trigger), out _);

                context.AutoResponses.RemoveRange(autoResponses);
                await context.SaveChangesAsync();
            }
        }

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.GuildId is null || e.Message.Author.IsBot)
                return;

            if (e.GuildId == SbuGlobals.Guild.SBU && e.Channel?.CategoryId == SbuGlobals.Channel.CATEGORY_SERIOUS)
                return;

            if (!_configService.GetValue(e.GuildId.Value, SbuGuildConfig.Respond))
                return;

            if (_autoResponses.GetValueOrDefault((e.GuildId.Value, e.Message.Content)) is { } response)
                await Bot.SendMessageAsync(e.ChannelId, new LocalMessage().WithContent(response));
        }
    }
}