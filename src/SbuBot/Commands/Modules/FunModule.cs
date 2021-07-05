using System.Linq;

using Disqord;
using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Modules
{
    public sealed class FunModule : SbuModuleBase
    {
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