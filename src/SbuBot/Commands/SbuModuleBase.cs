using System;
using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Qmmands;

using SbuBot.Commands.Views.Help;

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

        protected DiscordMenuCommandResult Help(Command command) => View(
            command.Description is { } ? new CommandView(command) : new GroupView(command.Module)
        );

        protected DiscordMenuCommandResult Help(IEnumerable<Command> commands)
        {
            return View(
                !commands.GroupBy(o => o.Name).Any()
                    ? new SearchMatchView(commands)
                    : new GroupView(commands.First().Module)
            );
        }

        protected DiscordMenuCommandResult Help(Module module) => View(new ModuleView(module));
    }
}