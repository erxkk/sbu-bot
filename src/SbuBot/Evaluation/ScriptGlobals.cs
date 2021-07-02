using System;
using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;

using Qmmands;

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
            message.WithReply(Context.Message.Id, Context.ChannelId, Context.GuildId, false)
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

        public DiscordMenuCommandResult Pages(IEnumerable<Page> pages) => Pages(new PageProvider(pages));

        public DiscordMenuCommandResult Pages(IPageProvider pageProvider) => View(new PagedView(pageProvider));

        public DiscordMenuCommandResult View(ViewBase view)
            => new(Context, new InteractiveMenu(Context.Author.Id) { View = view });

        public DiscordMenuCommandResult Menu(MenuBase menu) => new(Context, menu);

        // SbuModuleBase
        public DiscordMenuCommandResult FilledPages(
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

        public DiscordMenuCommandResult HelpView() => View(new RootView(Context.Bot.Commands));

        public DiscordMenuCommandResult HelpView(Command command) => View(
            command.Module.IsGroup() ? new GroupView(command.Module) : new CommandView(command)
        );

        public DiscordMenuCommandResult HelpView(IEnumerable<Command> commands)
        {
            return View(
                !commands.GroupBy(o => o.Module).HasAtLeast(2)
                    ? new SearchMatchView(commands)
                    : new GroupView(commands.First().Module)
            );
        }

        public DiscordMenuCommandResult HelpView(Module module)
            => View(module.IsGroup() ? new GroupView(module) : new ModuleView(module));
    }
}