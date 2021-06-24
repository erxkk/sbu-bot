using System;
using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Qmmands;

using SbuBot.Commands.Views;

namespace SbuBot.Commands
{
    public abstract class SbuModuleBase : DiscordGuildModuleBase
    {
        protected DiscordMenuCommandResult FilledPages(
            IEnumerable<string> contents,
            int itemsPerPage = -1,
            Func<LocalEmbed, LocalEmbed>? embedModifier = null
        ) => Pages(
            SbuUtility.FillPages(contents, itemsPerPage)
                .Select(p => new Page().WithEmbeds((embedModifier?.Invoke(new()) ?? new()).WithDescription(p)))
        );

        protected DiscordMenuCommandResult Help(Command command) => View(HelpView.Command(command));
        protected DiscordMenuCommandResult Help(Module module) => View(HelpView.Module(module));
    }
}