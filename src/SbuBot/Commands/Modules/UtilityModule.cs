using Disqord;
using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of utility commands.")]
    public sealed class UtilityModule : SbuModuleBase
    {
        [Command("ping")]
        [Description("Replies with `Pong!`.")]
        public DiscordCommandResult Ping() => Reply("Pong!");

        [Command("color")]
        [Description("Replies with the given color as an embed or a random color if non is given.")]
        public DiscordCommandResult ShowColor(
            [Description("The optional color to reply with.")]
            Color? color = null
        )
        {
            color ??= Color.Random;
            return Reply(new LocalEmbed().WithTitle(color.ToString()).WithColor(color));
        }
    }
}