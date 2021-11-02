using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;

using Qmmands;

using SbuBot.Commands.Views;
using SbuBot.Commands.Views.Help;

namespace SbuBot.Commands
{
    public abstract class SbuModuleBase : DiscordGuildModuleBase
    {
        protected override DiscordMenuCommandResult Pages(PageProvider pageProvider, TimeSpan timeout = default)
            => View(new CustomPagedView(pageProvider), timeout);

        protected DiscordMenuCommandResult Pages(params LocalEmbed[] embeds)
            => Pages(new ListPageProvider(embeds.Select(e => new Page().WithEmbeds(e))));

        protected DiscordMenuCommandResult DistributedPages(
            IEnumerable<string> contents,
            int itemsPerPage = -1,
            Func<LocalEmbed, LocalEmbed>? embedFactory = null
        ) => Pages(new DistributedPageProvider(contents, itemsPerPage, embedFactory));

        protected async Task<ConfirmationState> ConfirmationAsync(
            string prompt,
            string? description = null,
            TimeSpan timeout = default
        )
        {
            ConfirmationView confirmationView = new(prompt, description);

            try
            {
                await View(confirmationView, timeout != default ? timeout : TimeSpan.FromSeconds(30));
            }
            catch (OperationCanceledException)
            {
                return ConfirmationState.TimedOut;
            }

            return confirmationView.Result ? ConfirmationState.Confirmed : ConfirmationState.Aborted;
        }

        protected DiscordMenuCommandResult HelpView()
            => View(new RootHelpView(Context.Bot.Commands));

        protected DiscordMenuCommandResult HelpView(Command? command) => command is { }
            ? View(command.Module.IsGroup() ? new GroupHelpView(command.Module) : new CommandHelpView(command))
            : HelpView();

        protected DiscordMenuCommandResult HelpView(Module? module) => module is { }
            ? View(module.IsGroup() ? new GroupHelpView(module) : new ModuleHelpView(module))
            : HelpView();

        protected DiscordMenuCommandResult HelpView(object? commandOrModule) => commandOrModule is Command c
            ? HelpView(c)
            : commandOrModule is Module m
                ? HelpView(m)
                : HelpView();

        protected DiscordMenuCommandResult HelpView(IEnumerable<Module> modules)
            => modules.HasAtLeast(2)
                ? View(new SearchMatchHelpView(modules))
                : HelpView(modules.FirstOrDefault());

        protected DiscordMenuCommandResult HelpView(IEnumerable<Command> commands)
            => commands.HasAtLeast(2)
                ? commands.GroupBy(o => o.Module).HasAtLeast(2)
                    ? View(new SearchMatchHelpView(commands))
                    : HelpView(commands.First().Module)
                : HelpView(commands.FirstOrDefault());

        protected DiscordMenuCommandResult HelpView(IEnumerable<object> commandsOrModules)
            => commandsOrModules.HasAtLeast(2)
                ? View(new SearchMatchHelpView(commandsOrModules, true))
                : HelpView(commandsOrModules.FirstOrDefault());
    }
}