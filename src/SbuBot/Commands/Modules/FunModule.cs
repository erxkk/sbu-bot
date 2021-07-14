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
        public DiscordCommandResult Send() => Reply("Pong!");

        [Command("sex")]
        [Description("SEX!!!!!")]
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