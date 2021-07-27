using System.Linq;

using Disqord;
using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of fun commands.")]
    public sealed class FunModule : SbuModuleBase
    {
        [Command("ping")]
        [Description("Replies with `Pong!`.")]
        public DiscordCommandResult Ping() => Reply("Pong!");

        [Command("color")]
        [Description("Replies with the given color as an embed or a random color if non is given.")]
        public DiscordCommandResult ShowColor(
            [Description("The color to reply with.")]
            Color? color = null
        )
        {
            color ??= Color.Random;
            return Reply(new LocalEmbed().WithTitle(color.ToString()).WithColor(color));
        }

        [Command("sex")]
        [RequireGuild(SbuGlobals.Guild.Sbu.SELF)]
        [Description("SEX!!!!! (SECRET SBU ONLY COMMAND!!!!!!)")]
        public DiscordCommandResult Sex() => Reply(
            new LocalMessage()
                .WithContent("sex!!!")
                .WithComponents(
                    Enumerable.Repeat(
                        new LocalRowComponent().WithComponents(
                            Enumerable.Repeat(
                                new LocalLinkButtonComponent
                                {
                                    Label = "Sex!!!",
                                    Url = "https://knowyourmeme.com/memes/trollface",
                                },
                                5
                            )
                        ),
                        5
                    )
                )
        );
    }
}