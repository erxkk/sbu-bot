using System.ComponentModel;

using Disqord.Bot;
using Disqord.Gateway;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BotExtensions
    {
        // TODO: remove usages + fix usages by requiring a separator role or by restricting color roles to sbu
        // this is currently hardcoded and already used for guilds that don't contain this role
        public static CachedRole GetColorRoleSeparator(this DiscordBotBase @this) => @this.GetRole(
            SbuGlobals.Guild.SBU,
            SbuGlobals.Role.COLOR_SEPARATOR
        );
    }
}