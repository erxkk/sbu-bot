using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;
using Kkommon.Extensions.String;

using Qmmands;

using SbuBot.Commands;
using SbuBot.Commands.Views;
using SbuBot.Commands.Views.Help;
using SbuBot.Extensions;

namespace SbuBot.Evaluation
{
    public sealed class ScriptGlobals
    {
        public DiscordGuildCommandContext Context { get; }

        public ScriptGlobals(DiscordGuildCommandContext context) => Context = context;

        // DiscordModuleBase
        public DiscordResponseCommandResult Reply(string content)
            => Reply(new LocalMessage().WithContent(content));

        public DiscordResponseCommandResult Reply(params LocalEmbed[] embeds)
            => Reply(new LocalMessage().WithEmbeds(embeds));

        public DiscordResponseCommandResult Reply(string content, params LocalEmbed[] embeds)
            => Reply(new LocalMessage().WithContent(content).WithEmbeds(embeds));

        public DiscordResponseCommandResult Reply(LocalMessage message) => Response(
            message.WithReply(Context.Message.Id, Context.ChannelId, Context.GuildId)
        );

        public DiscordResponseCommandResult Response(string content)
            => Response(new LocalMessage().WithContent(content));

        public DiscordResponseCommandResult Response(params LocalEmbed[] embeds)
            => Response(new LocalMessage().WithEmbeds(embeds));

        public DiscordResponseCommandResult Response(string content, params LocalEmbed[] embeds) => Response(
            new LocalMessage().WithContent(content).WithEmbeds(embeds)
        );

        public DiscordResponseCommandResult Response(LocalMessage message)
        {
            message.AllowedMentions ??= LocalAllowedMentions.None;
            return new(Context, message);
        }

        public DiscordReactionCommandResult Reaction(LocalEmoji emoji) => new(Context, emoji);

        public DiscordMenuCommandResult Pages(params Page[] pages) => Pages((IEnumerable<Page>) pages);

        public DiscordMenuCommandResult Pages(IEnumerable<Page> pages) => Pages(new ListPageProvider(pages));

        public DiscordMenuCommandResult Pages(PageProvider pageProvider) => View(new CustomPagedView(pageProvider));

        public DiscordMenuCommandResult View(ViewBase view)
            => new(Context, new InteractiveMenu(Context.Author.Id, view));

        public DiscordMenuCommandResult Menu(MenuBase menu) => new(Context, menu);

        // SbuModuleBase

        public DiscordMenuCommandResult Pages(params LocalEmbed[] embeds)
            => Pages(new ListPageProvider(embeds.Select(e => new Page().WithEmbeds(e))));

        public DiscordMenuCommandResult DistributedPages(
            IEnumerable<string> contents,
            int itemsPerPage = -1,
            Func<LocalEmbed, LocalEmbed>? embedFactory = null
        ) => Pages(new DistributedPageProvider(contents, itemsPerPage, embedFactory));

        public async Task<ConfirmationState> ConfirmationAsync()
        {
            ConfirmationView confirmationView = new();
            InteractiveMenu menu = new(Context.Author.Id, confirmationView);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        public async Task<ConfirmationState> ConfirmationAsync(string prompt, string? description = null)
        {
            ConfirmationView confirmationView = new(prompt, description);
            InteractiveMenu menu = new(Context.Author.Id, confirmationView);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        // TODO: make interactive version + add to sbu module based
        public DiscordCommandResult Inspection(object? obj = null, int maxDepth = 2)
        {
            string inspection = obj?.GetInspection(maxDepth) ?? Context.GetInspection();

            // split earlier than max length to avoid huge embeds
            // TODO: method that splits at previous line instead of exact chunk size
            if (inspection.Length > 2048)
            {
                return Pages(
                    new ListPageProvider(
                        inspection.Chunk(2048).Select(c => new Page().WithEmbeds(new LocalEmbed().WithDescription(c)))
                    )
                );
            }

            return Response(new LocalEmbed().WithDescription(Markdown.CodeBlock("yml", inspection)));
        }

        public DiscordMenuCommandResult Help() => View(new RootView(Context.Bot.Commands));

        public DiscordMenuCommandResult Help(Command command) => View(
            command.Module.IsGroup() ? new GroupView(command.Module) : new CommandView(command)
        );

        public DiscordMenuCommandResult Help(IEnumerable<Command> commands)
        {
            return View(
                !commands.GroupBy(o => o.Module).HasAtLeast(2)
                    ? new SearchMatchView(commands)
                    : new GroupView(commands.First().Module)
            );
        }

        public DiscordMenuCommandResult Help(Module module)
            => View(module.IsGroup() ? new GroupView(module) : new ModuleView(module));
    }
}