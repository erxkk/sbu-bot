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

using SbuBot.Commands;
using SbuBot.Commands.Views;
using SbuBot.Commands.Views.Help;

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
            => new(Context, new DefaultMenu(view) { AuthorId = Context.Author.Id }, timeSpan);

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

        public async Task<ConfirmationState> ConfirmationAsync(
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

        public DiscordMenuCommandResult HelpView()
            => View(new RootHelpView(Context, Context.Bot.Commands));

        public DiscordMenuCommandResult HelpView(Command? command) => command is { }
            ? View(
                command.Module.IsGroup()
                    ? new GroupHelpView(Context, command.Module)
                    : new CommandHelpView(Context, command)
            )
            : HelpView();

        public DiscordMenuCommandResult HelpView(Module? module) => module is { }
            ? View(
                module.IsGroup()
                    ? new GroupHelpView(Context, module)
                    : new ModuleHelpView(Context, module)
            )
            : HelpView();

        public DiscordMenuCommandResult HelpView(object? commandOrModule) => commandOrModule switch
        {
            Command c => HelpView(c),
            Module m => HelpView(m),
            _ => HelpView(),
        };

        public DiscordMenuCommandResult HelpView(IEnumerable<Module> modules)
            => modules.HasAtLeast(2)
                ? View(new SearchMatchHelpView(Context, modules))
                : HelpView(modules.FirstOrDefault());

        public DiscordMenuCommandResult HelpView(IEnumerable<Command> commands)
            => commands.HasAtLeast(2)
                ? commands.GroupBy(o => o.Module).HasAtLeast(2)
                    ? View(new SearchMatchHelpView(Context, commands))
                    : HelpView(commands.First().Module)
                : HelpView(commands.FirstOrDefault());

        public DiscordMenuCommandResult HelpView(IEnumerable<object> commandsOrModules)
            => commandsOrModules.HasAtLeast(2)
                ? View(new SearchMatchHelpView(Context, commandsOrModules, true))
                : HelpView(commandsOrModules.FirstOrDefault());
    }
}
