using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

namespace SbuBot.Services
{
    public sealed class FunService : DiscordBotService
    {
        public override int Priority => int.MaxValue - 1;

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.Message.Author.Id == SbuGlobals.Bot.OWNER
                && e.Message.Content.Equals("ratio", StringComparison.OrdinalIgnoreCase))
                await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.Vote.UP));
        }
    }
}