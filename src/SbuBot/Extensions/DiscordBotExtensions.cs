using System.ComponentModel;

using Disqord.Bot;
using Disqord.Gateway;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordBotExtensions
    {
        public static CachedRole GetColorRoleSeparator(this DiscordBot @this) => @this.GetRole(
            SbuGlobals.Guild.SELF,
            SbuGlobals.Role.Color.SELF
        );
    }
}