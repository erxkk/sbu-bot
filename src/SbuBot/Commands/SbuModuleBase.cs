using System;
using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;

using Qmmands;

using SbuBot.Commands.Views;
using SbuBot.Commands.Views.Help;

namespace SbuBot.Commands
{
    public abstract class SbuModuleBase : DiscordGuildModuleBase
    {
        protected DiscordMenuCommandResult DistributedPages(
            IEnumerable<string> contents,
            int itemsPerPage = -1,
            Func<LocalEmbed, LocalEmbed>? embedFactory = null
        ) => Pages(new DistributedPageProvider(contents, itemsPerPage, embedFactory));

        protected override DiscordMenuCommandResult Pages(PageProvider pageProvider)
            => View(new CustomPagedView(pageProvider));

        public DiscordMenuCommandResult Pages(params LocalEmbed[] embeds)
            => Pages(new ListPageProvider(embeds.Select(e => new Page().WithEmbeds(e))));

        protected DiscordMenuCommandResult HelpView()
            => View(new RootView(Context.Bot.Commands));

        protected DiscordMenuCommandResult HelpView(Command command) => View(
            command.Module.IsGroup() ? new GroupView(command.Module) : new CommandView(command)
        );

        protected DiscordMenuCommandResult HelpView(IEnumerable<Command> commands) => View(
            !commands.GroupBy(o => o.Module).HasAtLeast(2)
                ? new SearchMatchView(commands)
                : new GroupView(commands.First().Module)
        );

        protected DiscordMenuCommandResult HelpView(Module module)
            => View(module.IsGroup() ? new GroupView(module) : new ModuleView(module));
    }
}