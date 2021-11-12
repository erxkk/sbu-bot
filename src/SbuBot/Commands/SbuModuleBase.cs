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

using SbuBot.Commands.Menus;
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
            int maxPageLength = LocalEmbed.MaxDescriptionLength,
            Func<LocalEmbed, LocalEmbed>? embedFactory = null
        ) => Pages(new DistributedPageProvider(contents, itemsPerPage, maxPageLength, embedFactory));

        protected Task<ConfirmationState> ConfirmationAsync(
            string prompt,
            string? description = null,
            TimeSpan timeout = default
        ) => ConfirmationAsync(Context.Author.Id, prompt, description, timeout);

        protected async Task<ConfirmationState> ConfirmationAsync(
            Snowflake targetId,
            string prompt,
            string? description = null,
            TimeSpan timeout = default
        )
        {
            ConfirmationView confirmationView = new(prompt, description);

            try
            {
                await Menu(
                    new DefaultMenu(confirmationView) { AuthorId = targetId },
                    timeout != default ? timeout : TimeSpan.FromSeconds(30)
                );
            }
            catch (OperationCanceledException)
            {
                return ConfirmationState.TimedOut;
            }

            return confirmationView.Result ? ConfirmationState.Confirmed : ConfirmationState.Aborted;
        }

        protected async Task<ConfirmationState> AgreementAsync(
            HashSet<Snowflake> targetIds,
            string prompt,
            string? description = null,
            TimeSpan timeout = default
        )
        {
            MultipleConfirmationView confirmationView = new(targetIds, prompt, description);

            try
            {
                await Menu(
                    new MultipleMenu(confirmationView, targetIds),
                    timeout != default ? timeout : TimeSpan.FromSeconds(30)
                );
            }
            catch (OperationCanceledException)
            {
                return ConfirmationState.TimedOut;
            }

            return confirmationView.Result ? ConfirmationState.Confirmed : ConfirmationState.Aborted;
        }

        protected DiscordMenuCommandResult HelpView()
            => View(new RootHelpView(Context, Context.Bot.Commands));

        protected DiscordMenuCommandResult HelpView(Command? command) => command is { }
            ? View(
                command.Module.IsGroup()
                    ? new GroupHelpView(Context, command.Module)
                    : new CommandHelpView(Context, command)
            )
            : HelpView();

        protected DiscordMenuCommandResult HelpView(Module? module) => module is { }
            ? View(
                module.IsGroup()
                    ? new GroupHelpView(Context, module)
                    : new ModuleHelpView(Context, module)
            )
            : HelpView();

        protected DiscordMenuCommandResult HelpView(object? commandOrModule) => commandOrModule switch
        {
            Command c => HelpView(c),
            Module m => HelpView(m),
            _ => HelpView(),
        };

        protected DiscordMenuCommandResult HelpView(IEnumerable<Module> modules)
            => modules.HasAtLeast(2)
                ? View(new SearchMatchHelpView(Context, modules))
                : HelpView(modules.FirstOrDefault());

        protected DiscordMenuCommandResult HelpView(IEnumerable<Command> commands)
            => commands.HasAtLeast(2)
                ? commands.GroupBy(o => o.Module).HasAtLeast(2)
                    ? View(new SearchMatchHelpView(Context, commands))
                    : HelpView(commands.First().Module)
                : HelpView(commands.FirstOrDefault());

        protected DiscordMenuCommandResult HelpView(IEnumerable<object> commandsOrModules)
            => commandsOrModules.HasAtLeast(2)
                ? View(new SearchMatchHelpView(Context, commandsOrModules, true))
                : HelpView(commandsOrModules.FirstOrDefault());
    }
}
