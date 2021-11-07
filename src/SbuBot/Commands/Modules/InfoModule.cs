using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of commands for help and general server/member/bot information.")]
    public sealed class InfoModule : SbuModuleBase
    {
        [Command("about")]
        [Description("Displays information about the bot.")]
        public async Task<DiscordCommandResult> AboutAsync()
        {
            IApplication application = await Context.Bot.FetchCurrentApplicationAsync();

            return Reply(
                new LocalEmbed()
                    .WithAuthor(application.Owner)
                    .WithTitle("Sbu-Bot")
                    .WithDescription("Bot for management of the sbu server.")
                    .AddInlineField("Prefix", Markdown.Code(SbuGlobals.DEFAULT_PREFIX))
                    .AddBlankInlineField()
                    .AddBlankInlineField()
                    .AddInlineField(
                        "Source",
                        Markdown.Link("Github:erxkk/sbu-bot", SbuGlobals.Links.GH_SELF)
                    )
                    .AddInlineField(
                        "Library",
                        Markdown.Link("Github:quahu/disqord", SbuGlobals.Links.GH_DISQORD)
                    )
                    .AddInlineField(
                        "CLR",
                        $".NET {Environment.Version}"
                    )
                    .AddInlineField(
                        "Host-OS",
                        Environment.OSVersion
                    )
            );
        }

        [Command("guide")]
        [Description("Displays an interactive guide that explains bot usage and help syntax.")]
        public DiscordCommandResult Guide() => Pages(
            new LocalEmbed()
                .WithTitle("Commands")
                .WithDescription(GuideStatic.COMMANDS),
            new LocalEmbed()
                .WithTitle("Syntax & Semantics")
                .WithDescription(GuideStatic.SYNTAX_SEMANTICS),
            new LocalEmbed()
                .WithTitle("Parsing")
                .WithDescription(GuideStatic.PARSING),
            new LocalEmbed()
                .WithTitle("Parsing Examples")
                .WithDescription(GuideStatic.PARSING_EXAMPLES),
            new LocalEmbed()
                .WithTitle("Escaping Examples")
                .WithDescription(GuideStatic.ESCAPING_EXAMPLES)
        );

        [Group("command", "commands")]
        [Description("A group of commands for displaying command information.")]
        public sealed class CommandSubModule : SbuModuleBase
        {
            [Command("modules")]
            [Description("Finds all commands that would match the given input.")]
            public DiscordCommandResult Modules() => HelpView();

            [Command("find")]
            [Description("Finds all commands that would match the given input.")]
            [Usage("command find role edit name", "command find help")]
            public DiscordCommandResult Find(string command)
            {
                IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(command);

                if (matches.Count == 0)
                    return Reply("Couldn't find any commands for the given input.");

                return DistributedPages(
                    matches.Select(m => m.Command)
                        .Select(
                            cmd => cmd.IsEnabled ? $"{SbuGlobals.BULLET} `{cmd.Format()}`" : $"~~`{cmd.Format()}`~~"
                        ),
                    embedFactory: embed => embed.WithTitle("Matched commands")
                );
            }

            [Command("list")]
            [Description("Lists all commands.")]
            public DiscordCommandResult List()
            {
                IReadOnlyList<Command> commands = Context.Bot.Commands.GetAllCommands();

                return DistributedPages(
                    commands.Select(
                        cmd => cmd.IsEnabled ? $"{SbuGlobals.BULLET} `{cmd.Format()}`" : $"~~`{cmd.Format()}`~~"
                    ),
                    embedFactory: embed => embed.WithTitle("Commands")
                );
            }
        }

        [Command("help")]
        [Description(
            "Interactively displays information about for a given command/module, or displays all modules if non is "
            + "given."
        )]
        [Usage("help role edit name", "how role edit", "h role")]
        public DiscordCommandResult Help(string? command = null)
        {
            if (command is null)
                return HelpView();

            IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(command);

            switch (matches.Count)
            {
                case 0:
                {
                    Module[] moduleMatches = Context.Bot.Commands.GetAllModules()
                        .Where(c => c.Aliases.Any(a => a.Equals(command, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();

                    Command[] commandMatches = Context.Bot.Commands.GetAllCommands()
                        .Where(c => c.Aliases.Any(a => a.Equals(command, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();

                    switch ((moduleMatches.Length, commandMatches.Length))
                    {
                        case (>= 1, >= 1):
                            return HelpView(Enumerable.Empty<object>().Concat(moduleMatches).Concat(commandMatches));

                        case (0, >= 1):
                            return HelpView(commandMatches);

                        case (>= 1, 0):
                            return HelpView(moduleMatches);

                        default:
                            return HelpView();
                    }
                }

                case 1:
                    return HelpView(matches[0].Command);

                default:
                    return HelpView(matches.Select(c => c.Command));
            }
        }

        private static class GuideStatic
        {
            private const string INDENT = "\u200B \u200B \u200B \u200B ";

            public const string COMMANDS
                = "To use Commands ping the bot or send a message that starts with `sbu`, the space between the prefix "
                + "and the command is optional and does not influence the command execution, both `sbu ping` and "
                + "`sbuping` will work just fine.";

            public static readonly string SYNTAX_SEMANTICS
                = "**Brackets in help commands indicate parameter importance**\n"
                + "• `<param>` a required parameter, it cannot be left out\n"
                + "• `[param = default]` an optional parameter, if left out the default value will be used\n"
                + "• `[params…]` a collection of values, if left out none will be passed\n"
                + "• `{object}` non-string-literals\n"
                + $"{GuideStatic.INDENT}- `{{-cmd}}` invokes `sbu cmd` instead\n"
                + $"{GuideStatic.INDENT}- `{{!state}}` negation of state\n"
                + $"{GuideStatic.INDENT}- `{{@user}}` user-mention\n"
                + $"{GuideStatic.INDENT}- `{{@reply}}` inline-reply\n"
                + $"{GuideStatic.INDENT}- `{{#channel}}` channel-mention\n"
                + "\n"
                + "Arguments are separated by spaces, wrap an argument in quotes `\"\"` if it contains spaces, this is "
                + "not necessary for the last non-collection parameter.";

            public const string PARSING
                = "**Quotes and backslashes receive special handling when parsing**\n"
                + "• Quotes `\"counts as one\"` indicate the start and end of an argument that contains spaces and is "
                + "not the last argument, they are parsed normally on the last argument.\n"
                + "• Backslashes escape the following character to not receive any special handling.\n"
                + "• To use quotes or slashes as literal values anywhere they have to be escaped `\\\"` will be parsed "
                + "as `\"`.\n"
                + "• Descriptors are used to make parsing easier, a descriptor separates arguments by `::` and "
                + "discards leading and trailing whitspace.";

            public static readonly string PARSING_EXAMPLES
                = "• **Optional argument**\n"
                + $"{GuideStatic.INDENT}- command `ban <user> [reason = \"beaned\"]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu ban @joemama`\n"
                + $"{GuideStatic.INDENT}- bans `@joemama` with `beaned` as the reason\n"
                + "\n"
                + "• **Optional argument not omitted**\n"
                + $"{GuideStatic.INDENT}- command `ban <user> [reason = \"beaned\"]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu ban @joemama you're a retard`\n"
                + $"{GuideStatic.INDENT}- bans `@joemama` with `you're a retard` as the reason\n"
                + "\n"
                + "• **Additional argument omitted**\n"
                + $"{GuideStatic.INDENT}- command `gift <user> <tag> [additional tags…]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu gift @joemama tag1`\n"
                + $"{GuideStatic.INDENT}- gifts `@joemama` `tag1`\n"
                + "\n"
                + "• **Additional argument not omitted**\n"
                + $"{GuideStatic.INDENT}- command `gift <user> <tag> [additional tags…]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu gift @joemama tag1 \"tag2 with spaces\" tag3`\n"
                + $"{GuideStatic.INDENT}- gifts `@joemama` `tag1`, `tag2 with spaces and tag3`";

            public static readonly string ESCAPING_EXAMPLES
                = "• **Escaping a quote to include it in the argument**\n"
                + $"{GuideStatic.INDENT}- command `tag <name> <conent>`\n"
                + $"{GuideStatic.INDENT}- used like `sbu tag \\\"\\\"\\\"them\\\"\\\"\\\" ||da juice||`\n"
                + $"{GuideStatic.INDENT}- creates a tag with `\"\"\"them\"\"\"` as name and `||da juice||` as content\n"
                + "\n"
                + "• **Using a descriptor for the same tag**\n"
                + $"{GuideStatic.INDENT}- command `tag <tagDescriptor>`\n"
                + $"{GuideStatic.INDENT}- used like `sbu tag \"\"\"them\"\"\" :: ||da juice||`\n"
                + $"{GuideStatic.INDENT}- creates a tag with `\"\"\"them\"\"\"` as name and `||da juice||` as content";
        }
    }
}