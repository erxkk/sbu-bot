using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class SbuService : DiscordBotService
    {
        public override int Priority => int.MaxValue - 2;

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.GuildId is null)
                return;

            if (e.GuildId != SbuGlobals.Guild.Sbu.SELF)
                return;

            switch (e.Message.Author.Id)
            {
                case SbuGlobals.Bot.OWNER when e.Message.Content.Equals("ratio", StringComparison.OrdinalIgnoreCase):
                    await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.Vote.UP));
                    return;

                case SbuGlobals.Member.TOASTY when e.Message.Content.Equals("cum", StringComparison.OrdinalIgnoreCase):
                    await e.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.Misc.CUM));
                    return;
            }
        }
    }
}