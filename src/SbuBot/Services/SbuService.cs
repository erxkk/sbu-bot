using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

namespace SbuBot.Services
{
    public sealed class SbuService : DiscordBotService
    {
        public override int Priority => int.MaxValue - 2;

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.GuildId != SbuGlobals.Guild.SBU
                || e.Channel is not ICategorizableGuildChannel categorizable
                || categorizable.CategoryId == SbuGlobals.Channel.CATEGORY_SERIOUS)
                return;

            switch (e.Message.Author.Id)
            {
                case SbuGlobals.Users.ERXKK when e.Message.Content.Equals("ratio", StringComparison.OrdinalIgnoreCase):
                    await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.Vote.UP));
                    return;

                case SbuGlobals.Users.TOASTY when e.Message.Content.Equals("cum", StringComparison.OrdinalIgnoreCase):
                    await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.CUM));
                    return;
            }
        }
    }
}