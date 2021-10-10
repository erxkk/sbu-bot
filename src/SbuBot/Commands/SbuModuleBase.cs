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

        protected async Task<ConfirmationState> ConfirmationAsync()
        {
            ConfirmationView confirmationView = new();
            DefaultMenu menu = new(confirmationView, Context.Author.Id);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        protected async Task<ConfirmationState> ConfirmationAsync(string prompt, string? description = null)
        {
            ConfirmationView confirmationView = new(prompt, description);
            DefaultMenu menu = new(confirmationView, Context.Author.Id);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        protected DiscordMenuCommandResult Help()
            => View(new RootHelpView(Context.Bot.Commands));

        protected DiscordMenuCommandResult Help(Command command) => View(
            command.Module.IsGroup() ? new GroupHelpView(command.Module) : new CommandHelpView(command)
        );

        protected DiscordMenuCommandResult Help(IEnumerable<Command> commands) => View(
            !commands.GroupBy(o => o.Module).HasAtLeast(2)
                ? new SearchMatchHelpView(commands)
                : new GroupHelpView(commands.First().Module)
        );

        protected DiscordMenuCommandResult Help(Module module)
            => View(module.IsGroup() ? new GroupHelpView(module) : new ModuleHelpView(module));
    }
}