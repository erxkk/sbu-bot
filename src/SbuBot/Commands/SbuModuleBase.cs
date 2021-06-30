using System;
using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;

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

        protected DiscordMenuCommandResult HelpView() => View(new RootView(Context.Bot.Commands));

        protected DiscordMenuCommandResult HelpView(Command command) => View(
            command.Module.IsGroup() ? new CommandView(command) : new GroupView(command.Module)
        );

        protected DiscordMenuCommandResult HelpView(IEnumerable<Command> commands)
        {
            return View(
                !commands.GroupBy(o => o.Name).HasAtLeast(2)
                    ? new SearchMatchView(commands)
                    : new GroupView(commands.First().Module)
            );
        }

        protected DiscordMenuCommandResult HelpView(Module module)
            => View(module.IsGroup() ? new GroupView(module) : new ModuleView(module));
    }
}