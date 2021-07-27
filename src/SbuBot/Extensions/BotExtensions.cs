using System.ComponentModel;

using Disqord.Bot;
using Disqord.Gateway;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BotExtensions
    {
        public static CachedRole GetColorRoleSeparator(this DiscordBotBase @this) => @this.GetRole(
            SbuGlobals.Guild.Sbu.SELF,
            SbuGlobals.Guild.Sbu.Role.Color.SELF
        );

        public static CachedChannel GetPinArchive(this DiscordBotBase @this) => @this.GetChannel(
            SbuGlobals.Guild.Sbu.SELF,
            SbuGlobals.Guild.Sbu.Channel.Based.PIN_ARCHIVE
        );
    }
}