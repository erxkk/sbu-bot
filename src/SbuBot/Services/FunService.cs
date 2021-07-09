using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class FunService : DiscordBotService
    {
        private readonly ConfigService _configService;
        public override int Priority => int.MaxValue - 2;

        public FunService(ConfigService configService) => _configService = configService;

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.GuildId is null)
                return;

            if (!_configService.GetValue(e.GuildId.Value, SbuGuildConfig.Fun))
                return;

            switch (e.Message.Author.Id)
            {
                case SbuGlobals.Bot.OWNER when e.Message.Content.Equals("ratio", StringComparison.OrdinalIgnoreCase):
                    await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.Vote.UP));
                    break;

                case SbuGlobals.Member.TOASTY when e.Message.Content.Equals("cum", StringComparison.OrdinalIgnoreCase):
                    await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.CUM));
                    break;
            }
        }
    }
}