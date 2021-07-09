using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ChatService : SbuBotServiceBase
    {
        private readonly ConfigService _configService;

        public override int Priority => int.MaxValue - 1;

        public ChatService(SbuConfiguration configuration, ConfigService configService) : base(configuration)
            => _configService = configService;

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (!Configuration.IsProduction)
                return;

            if (e.GuildId is null)
                return;

            if (!_configService.GetValue(e.GuildId.Value, SbuGuildConfig.Chat))
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetGuildAsync(e.GuildId.Value) is not { } guild)
                    return;

                if (guild.Config.HasFlag(SbuGuildConfig.Chat))
                    return;
            }

            const string mediaDomain = "media.discordapp.net";
            const string cdnDomain = "cdn.discordapp.com";

            if (e.Message.Content.Contains(mediaDomain))
            {
                await Bot.SendMessageAsync(
                    e.ChannelId,
                    new LocalMessage().WithContent(
                        string.Format(
                            "{0} use `{1}` instead of `{2}`, the latter is broken:\n{3}",
                            e.Message.Author.Mention,
                            cdnDomain,
                            mediaDomain,
                            e.Message.Content.Replace(mediaDomain, cdnDomain)
                        )
                    )
                );

                await e.Message.DeleteAsync();
            }
        }
    }
}