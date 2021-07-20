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
        protected override DiscordMenuCommandResult Pages(PageProvider pageProvider)
            => View(new CustomPagedView(pageProvider));

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
            InteractiveMenu menu = new(Context.Author.Id, confirmationView);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        protected async Task<ConfirmationState> ConfirmationAsync(string prompt, string? description = null)
        {
            ConfirmationView confirmationView = new(prompt, description);
            InteractiveMenu menu = new(Context.Author.Id, confirmationView);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        protected DiscordMenuCommandResult Help()
            => View(new RootView(Context.Bot.Commands));

        protected DiscordMenuCommandResult Help(Command command) => View(
            command.Module.IsGroup() ? new GroupView(command.Module) : new CommandView(command)
        );

        protected DiscordMenuCommandResult Help(IEnumerable<Command> commands) => View(
            !commands.GroupBy(o => o.Module).HasAtLeast(2)
                ? new SearchMatchView(commands)
                : new GroupView(commands.First().Module)
        );

        protected DiscordMenuCommandResult Help(Module module)
            => View(module.IsGroup() ? new GroupView(module) : new ModuleView(module));
    }
}