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
        // TODO: could move FillPages to command utility and use ArrayPageProvider with modified FillPages?
        protected DiscordMenuCommandResult FilledPages(
            IEnumerable<string> contents,
            int itemsPerPage = -1,
            Func<LocalEmbed, LocalEmbed>? embedModifier = null
        ) => Pages(
            SbuUtility.FillPages(contents, itemsPerPage)
                .Select(p => new Page().WithEmbeds((embedModifier?.Invoke(new()) ?? new()).WithDescription(p)))
        );

        public DiscordMenuCommandResult CountedPages(params LocalEmbed[] embeds)
            => Pages(
                new ArrayPageProvider<LocalEmbed>(
                    embeds,
                    (view, item) => new Page().WithEmbeds(
                        item[0].WithFooter($"{view.CurrentPageIndex + 1}/{view.PageProvider.PageCount}")
                    ),
                    1
                )
            );

        protected DiscordMenuCommandResult HelpView() => View(new RootView(Context.Bot.Commands));

        protected DiscordMenuCommandResult HelpView(Command command) => View(
            command.Module.IsGroup() ? new GroupView(command.Module) : new CommandView(command)
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