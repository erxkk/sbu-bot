using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace SbuBot.Commands
{
    public abstract class SbuModuleBase : DiscordGuildModuleBase
    {
        // TODO: rework with new Menus
        protected DiscordMenuCommandResult FilledPages(
            IEnumerable<string> contents,
            int itemsPerPage = -1
        ) => Pages(
            SbuUtility.FillPages(contents, itemsPerPage)
                .Select(p => new Page().WithEmbeds(new LocalEmbed().WithDescription(p)))
        );
    }
}