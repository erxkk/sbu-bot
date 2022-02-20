using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Kkommon.Extensions.Prototyping;

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
            IApplication application = await Bot.FetchCurrentApplicationAsync();

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
                        Markdown.Link("Github:erxkk/sbu-bot", SbuGlobals.Link.GH_SELF)
                    )
                    .AddInlineField(
                        "Library",
                        Markdown.Link("Github:Quahu/Disqord", SbuGlobals.Link.GH_DISQORD)
                    )
                    .AddBlankInlineField()
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

        [Command("reserved")]
        [Description(
            "Lists the reserved keywords, tags are not allowed to be any of these keywords, but can start with, end "
            + "with or contain them."
        )]
        public DiscordCommandResult GetReservedKeywords() => Reply(
            string.Format(
                "The following keywords are not allowed to be tags, but tags may contain them:\n{0}",
                SbuGlobals.Keyword.ALL_RESERVED.Select(rn => $"> `{rn}`").ToNewLines()
            )
        );

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

        [Group("command", "commands", "cmd")]
        [Description("A group of commands for displaying command information.")]
        public sealed class CommandSubModule : SbuModuleBase
        {
            [Command("find")]
            [Description("Finds all commands that would match the given input.")]
            [UsageOverride("command find role edit name", "command find help")]
            public DiscordCommandResult Find(string command)
            {
                IReadOnlyList<CommandMatch> matches = Bot.Commands.FindCommands(command);

                if (matches.Count == 0)
                    return Reply("Couldn't find any commands for the given input.");

                return DistributedPages(
                    matches.Select(m => m.Command)
                        .Select(
                            cmd => cmd.IsEnabled ? $"{SbuGlobals.BULLET} `{cmd.Format()}`" : $"~~`{cmd.Format()}`~~"
                        ),
                    maxPageLength: LocalEmbed.MaxDescriptionLength / 2,
                    embedFactory: embed => embed.WithTitle("Matched commands"),
                    itemsPerPage: 20
                );
            }

            // BUG: modules return command + sub module == 0 even though they do have some
            // when inspected they behave fine
            // GuildManagementModule
            //     ArchiveSubModule
            //     ConfigSubModule
            //     RequestSubModule
            [Command("list")]
            [Description("Lists all commands.")]
            public DiscordCommandResult List()
            {
                IReadOnlySet<Module> modules = Bot.Commands.TopLevelModules;

                static IEnumerable<string> children(Module module, int depth = 1)
                {
                    return module.Submodules.Select(
                            subModule => string.Format(
                                "{0}{1} {2}{3}{4}",
                                string.Join("", Enumerable.Repeat(GuideStatic.INDENT, depth)),
                                SbuGlobals.BULLET_2,
                                module.IsEnabled ? subModule.Name : $"~~{subModule.Name}~~",
                                module.IsEnabled && subModule.Submodules.Count + module.Commands.Count != 0 ? "\n" : "",
                                module.IsEnabled && subModule.Submodules.Count + module.Commands.Count != 0
                                    ? children(subModule, depth + 1).ToNewLines()
                                    : null
                            )
                        )
                        .Concat(
                            module.Commands.Select(
                                command => string.Format(
                                    "{0}{1} {2}",
                                    string.Join("", Enumerable.Repeat(GuideStatic.INDENT, depth)),
                                    SbuGlobals.BULLET,
                                    module.IsEnabled ? $"`{command.Format(false)}`" : $"~~`{command.Format(false)}`~~"
                                )
                            )
                        );
                }

                return DistributedPages(
                    modules.Select(
                        module => string.Format(
                            "{0} {1}{2}{3}\n",
                            SbuGlobals.BULLET_2,
                            module.IsEnabled ? module.Name : $"~~{module.Name}~~",
                            module.IsEnabled && module.Submodules.Count + module.Commands.Count != 0 ? "\n" : "",
                            module.IsEnabled && module.Submodules.Count + module.Commands.Count != 0
                                ? children(module).ToNewLines()
                                : null
                        )
                    ),
                    maxPageLength:
                    LocalEmbed.MaxDescriptionLength / 2,
                    embedFactory:
                    embed => embed.WithTitle("Commands")
                );
            }
        }

        [Group("module", "modules", "mod")]
        [Description("A group of commands for displaying module information.")]
        public sealed class ModuleSubModule : SbuModuleBase
        {
            [Command]
            [Description("Finds all commands that would match the given input.")]
            public DiscordCommandResult Modules() => HelpView();

            [Command("list")]
            [Description("Lists all modules.")]
            public DiscordCommandResult List()
            {
                IReadOnlySet<Module> modules = Bot.Commands.TopLevelModules;

                static IEnumerable<string> subModules(Module module, int depth = 1)
                {
                    return module.Submodules.Select(
                        subModule => string.Format(
                            "{0}{1} {2}{3}{4}",
                            string.Join("", Enumerable.Repeat(GuideStatic.INDENT, depth)),
                            SbuGlobals.BULLET,
                            subModule.IsEnabled ? subModule.Name : $"~~{subModule.Name}~~",
                            subModule.IsEnabled && subModule.Submodules.Count != 0 ? "\n" : "",
                            subModule.IsEnabled && subModule.Submodules.Count != 0
                                ? subModules(subModule, depth + 1).ToNewLines()
                                : null
                        )
                    );
                }

                return DistributedPages(
                    modules.Select(
                        module => string.Format(
                            "{0} {1}{2}{3}\n",
                            SbuGlobals.BULLET,
                            module.IsEnabled ? module.Name : $"~~{module.Name}~~",
                            module.IsEnabled && module.Submodules.Count != 0 ? "\n" : "",
                            module.IsEnabled && module.Submodules.Count != 0 ? subModules(module).ToNewLines() : null
                        )
                    ),
                    maxPageLength: LocalEmbed.MaxDescriptionLength / 2,
                    embedFactory: embed => embed.WithTitle("Modules")
                );
            }
        }

        [Command("help")]
        [Description("Interactively displays information about for a given command/module, or displays general help.")]
        public DiscordCommandResult Help(string? path = null)
        {
            if (path is null)
            {
                return Reply(
                    new LocalEmbed().WithDescription(
                        $"{SbuGlobals.BULLET} Use `sbu help [command path]` to get info about a specific command.\n"
                        + $"{SbuGlobals.BULLET} Use `sbu module list` to get a complete list of modules.\n"
                        + $"{SbuGlobals.BULLET} Use `sbu command list` to get a complete list of commands.\n"
                        + "\n"
                        + "A command/module path is just the full name of a command without the prefix.\n"
                        + "For a command used like this: `sbu db inspect @role` the path is `db inspect`."
                    )
                );
            }

            int maxLength = 0;

            IReadOnlyList<CommandMatch> matches = Bot.Commands.FindCommands(path)
                .OrderByDescending(m => m.Path.Count)
                .TakeWhile(
                    m =>
                    {
                        maxLength = Math.Max(m.Path.Count, maxLength);
                        return m.Path.Count >= maxLength;
                    }
                )
                .ToList();

            switch (matches.Count)
            {
                case 0:
                {
                    Module[] moduleMatches = Bot.Commands.GetAllModules()
                        .Where(m => m.Aliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();

                    Command[] commandMatches = Bot.Commands.GetAllCommands()
                        .Where(c => c.Aliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase)))
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

        [Command("example")]
        public DiscordCommandResult Example(string path)
        {
            int maxLength = 0;

            IReadOnlyList<CommandMatch> matches = Bot.Commands.FindCommands(path)
                .OrderByDescending(m => m.Path.Count)
                .TakeWhile(
                    m =>
                    {
                        maxLength = Math.Max(m.Path.Count, maxLength);
                        return m.Path.Count >= maxLength;
                    }
                )
                .ToList();

            object[] allMatches = (matches.Count) switch
            {
                0 => Enumerable.Empty<object>()
                    .Concat(
                        Bot.Commands.GetAllModules()
                            .Where(m => m.Aliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    )
                    .Concat(
                        Bot.Commands.GetAllCommands()
                            .Where(c => c.Aliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    )
                    .ToArray(),

                _ => matches.Select(c => c.Command as object).ToArray(),
            };

            if (allMatches.Length == 0)
                return Reply($"No commands or modules found for `{path}`");

            return DistributedPages(
                allMatches.Select(
                        com =>
                        {
                            return com switch
                            {
                                Module m => (m.Format(), Usage.GetUsages(m)),
                                Command c => (c.Format(), Usage.GetUsages(c)),
                                _ => throw new ArgumentOutOfRangeException()
                            };
                        }
                    )
                    .Select(
                        formatAndUsage
                            => string.Format(
                                "`sbu {0}`\n{1}",
                                formatAndUsage.Item1,
                                Markdown.CodeBlock(
                                    "md",
                                    formatAndUsage.Item2.Select(u => $"{SbuGlobals.BULLET} {u}").ToNewLines()
                                )
                            )
                    ),
                maxPageLength: LocalEmbed.MaxDescriptionLength / 2,
                embedFactory: embed => embed.WithTitle("Examples")
            );
        }

        private static class GuideStatic
        {
            public const string INDENT = "\u200B \u200B \u200B \u200B ";

            public const string COMMANDS
                = "To use Commands ping the bot or send a message that starts with `sbu`, the space between the prefix "
                + "and the command is optional and does not influence the command execution, both `sbu ping` and "
                + "`sbuping` will work just fine.";

            public static readonly string SYNTAX_SEMANTICS
                = "**Brackets in help commands indicate parameter importance**\n"
                + $"{SbuGlobals.BULLET} `<param>` a required parameter, it cannot be left out\n"
                + $"{SbuGlobals.BULLET} `[param = default]` an optional parameter, if left out the default value will "
                + "be used\n"
                + $"{SbuGlobals.BULLET} `[params…]` a collection of values, if left out none will be passed\n"
                + $"{SbuGlobals.BULLET} `{{object}}` non-string-literals\n"
                + $"{GuideStatic.INDENT}- `{{-cmd}}` invokes `sbu cmd` instead\n"
                + $"{GuideStatic.INDENT}- `{{!state}}` negation of state\n"
                + $"{GuideStatic.INDENT}- `{{@user}}` user-mention\n"
                + $"{GuideStatic.INDENT}- `{{@reply}}` inline-reply\n"
                + $"{GuideStatic.INDENT}- `{{#channel}}` channel-mention\n"
                + "\n"
                + "Arguments are separated by spaces, wrap an argument in quotes `\"\"` if it contains spaces, this is "
                + "not necessary for the last non-collection parameter.";

            public static readonly string PARSING
                = "**Quotes and backslashes receive special handling when parsing**\n"
                + $"{SbuGlobals.BULLET} Quotes `\"counts as one\"` indicate the start and end of an argument that "
                + "contains spaces and is "
                + "not the last argument, they are parsed normally on the last argument.\n"
                + $"{SbuGlobals.BULLET} Backslashes escape the following character to not receive any special "
                + $"handling.\n"
                + $"{SbuGlobals.BULLET} To use quotes or slashes as literal values anywhere they have to be escaped "
                + $"`\\\"` will be parsed as `\"`.\n"
                + $"{SbuGlobals.BULLET} Descriptors are used to make parsing easier, a descriptor separates arguments "
                + $"by `{SbuGlobals.DESCRIPTOR_SEPARATOR}` and discards leading and trailing whitespace.";

            public static readonly string PARSING_EXAMPLES
                = $"{SbuGlobals.BULLET} **Optional argument**\n"
                + $"{GuideStatic.INDENT}- command `ban <user> [reason = \"beaned\"]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu ban @joemama`\n"
                + $"{GuideStatic.INDENT}- bans `@joemama` with `beaned` as the reason\n"
                + "\n"
                + $"{SbuGlobals.BULLET} **Optional argument not omitted**\n"
                + $"{GuideStatic.INDENT}- command `ban <user> [reason = \"beaned\"]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu ban @joemama you're a retard`\n"
                + $"{GuideStatic.INDENT}- bans `@joemama` with `you're a retard` as the reason\n"
                + "\n"
                + $"{SbuGlobals.BULLET} **Additional argument omitted**\n"
                + $"{GuideStatic.INDENT}- command `gift <user> <tag> [additional tags…]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu gift @joemama tag1`\n"
                + $"{GuideStatic.INDENT}- gifts `@joemama` `tag1`\n"
                + "\n"
                + $"{SbuGlobals.BULLET} **Additional argument not omitted**\n"
                + $"{GuideStatic.INDENT}- command `gift <user> <tag> [additional tags…]`\n"
                + $"{GuideStatic.INDENT}- used like `sbu gift @joemama tag1 \"tag2 with spaces\" tag3`\n"
                + $"{GuideStatic.INDENT}- gifts `@joemama` `tag1`, `tag2 with spaces and tag3`";

            public static readonly string ESCAPING_EXAMPLES
                = $"{SbuGlobals.BULLET} **Escaping a quote to include it in the argument**\n"
                + $"{GuideStatic.INDENT}- command `tag <name> <conent>`\n"
                + $"{GuideStatic.INDENT}- used like `sbu tag \\\"\\\"\\\"them\\\"\\\"\\\" ||da juice||`\n"
                + $"{GuideStatic.INDENT}- creates a tag with `\"\"\"them\"\"\"` as name and `||da juice||` as content\n"
                + "\n"
                + $"{SbuGlobals.BULLET} **Using a descriptor for the same tag**\n"
                + $"{GuideStatic.INDENT}- command `tag <tagDescriptor>`\n"
                + $"{GuideStatic.INDENT}- used like `sbu tag \"\"\"lmaoo\"\"\" {SbuGlobals.DESCRIPTOR_SEPARATOR} "
                + "||not funny||`\n"
                + $"{GuideStatic.INDENT}- creates a tag with `\"\"\"lmaoo\"\"\"` as name and `||not funny||` as "
                + "content";
        }
    }
}
