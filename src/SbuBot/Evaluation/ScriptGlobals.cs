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

        public DiscordMenuCommandResult Pages(params Page[] pages) => Pages((IEnumerable<Page>)pages);

        public DiscordMenuCommandResult Pages(IEnumerable<Page> pages, TimeSpan timeSpan = default)
            => Pages(new ListPageProvider(pages), timeSpan);

        public DiscordMenuCommandResult Pages(PageProvider pageProvider, TimeSpan timeSpan = default)
            => View(new CustomPagedView(pageProvider), timeSpan);

        public DiscordMenuCommandResult View(ViewBase view, TimeSpan timeSpan = default)
            => new(Context, new DefaultMenu(view, Context.Author.Id), timeSpan);

        public DiscordMenuCommandResult Menu(MenuBase menu, TimeSpan timeSpan = default)
            => new(Context, menu, timeSpan);

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
            DefaultMenu menu = new(confirmationView, Context.Author.Id);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        public async Task<ConfirmationState> ConfirmationAsync(string prompt, string? description = null)
        {
            ConfirmationView confirmationView = new(prompt, description);
            DefaultMenu menu = new(confirmationView, Context.Author.Id);

            await Context.Bot.StartMenuAsync(Context.ChannelId, menu, TimeSpan.FromMinutes(1));
            await menu.Task;

            return confirmationView.State;
        }

        public DiscordCommandResult Inspection(object? obj = null, int maxDepth = 2)
        {
            string inspection = obj?.GetInspection(maxDepth) ?? Context.GetInspection();

            // split earlier than max length to avoid huge embeds
            // TODO: context aware splitting
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

        public DiscordMenuCommandResult Help() => View(new RootHelpView(Context.Bot.Commands));

        public DiscordMenuCommandResult Help(Command command) => View(
            command.Module.IsGroup() ? new GroupHelpView(command.Module) : new CommandHelpView(command)
        );

        public DiscordMenuCommandResult Help(IEnumerable<Command> commands)
        {
            return View(
                !commands.GroupBy(o => o.Module).HasAtLeast(2)
                    ? new SearchMatchHelpView(commands)
                    : new GroupHelpView(commands.First().Module)
            );
        }

        public DiscordMenuCommandResult Help(Module module)
            => View(module.IsGroup() ? new GroupHelpView(module) : new ModuleHelpView(module));
    }
}