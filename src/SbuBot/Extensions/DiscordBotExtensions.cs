using System.ComponentModel;

using Disqord.Bot;
using Disqord.Gateway;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordBotExtensions
    {
        public static CachedRole GetColorRoleSeparator(this DiscordBotBase @this) => @this.GetRole(
            SbuGlobals.Guild.SELF,
            SbuGlobals.Role.Color.SELF
        );

        public static CachedChannel GetPinArchive(this DiscordBotBase @this) => @this.GetChannel(
            SbuGlobals.Guild.SELF,
            SbuGlobals.Channel.Based.PIN_ARCHIVE
        );
    }
}