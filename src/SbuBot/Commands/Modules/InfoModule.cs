using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon.Extensions.AsyncEnumerable;

using Qmmands;

using SbuBot.Commands.Information;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    // TODO: annotate when finished
    // TODO: improve syntax explanation, add descriptor explanation
    // TODO: add dynamic menu with navigation when buttons are finished
    // TODO: add informational commands
    [Description("A collection of commands for help and general server/member/bot information."), Disabled]
    public sealed class InfoModule : SbuModuleBase
    {
        [Command("about")]
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
                    .AddInlineField("Version", SbuGlobals.VERSION)
                    .AddInlineField(
                        "Source",
                        Markdown.Link("Github:erxkk/sbu-bot", SbuGlobals.Github.SELF)
                    )
                    .AddInlineField(
                        "Library",
                        Markdown.Link("Github:Quahu/Disqord", SbuGlobals.Github.DISQORD)
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
        public DiscordCommandResult Guide() => Pages(
            new Page().WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Commands")
                    .WithFooter("1/5")
                    .WithDescription(
                        "To use Commands ping the bot or send a message that starts with "
                        + $"'{SbuGlobals.DEFAULT_PREFIX}', the space between the prefix and the command is "
                        + "optional and does not influence the command execution, both `sbu ping` and "
                        + "`sbuping` will work just fine."
                    )
            ),
            new Page().WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Parameters")
                    .WithFooter("2/5")
                    .WithDescription(
                        "Brackets in help commands indicate parameter importance:"
                        + "\n> - `<param>` indicates a required parameter, it cannot be left out."
                        + "\n> - `[param = default]` indicates an optional parameter, if left out the default value "
                        + "will be used."
                        + "\n> - `[params…]` indicates a collection of values, if left out none will be passed."
                        + "\n> - All parameters but the last of each command are separated by spaces by default."
                    )
            ),
            new Page().WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Parameters Examples")
                    .WithFooter("3/5")
                    .WithDescription(
                        "> `ban <user> [reason = \"beaned\"]` can be used like:\n"
                        + Markdown.CodeBlock("sbu ban @joemama\nsbu ban @joemama you're a jew")
                        + "\n> `gift <user> <tag> [additional tags…]` can be used like:\n"
                        + Markdown.CodeBlock("sbu gift @joemama tag1\nsbu gift @joemama tag1 tag2 tag3")
                    )
            ),
            new Page().WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Parsing")
                    .WithFooter("4/5")
                    .WithDescription(
                        "Quotes and backslashes receive special handling when parsing:\n"
                        + "> - Quotes `\"counts as one\"` indicate the start and end of an argument that contains "
                        + "spaces and is not the last argument, they are ignored on the last argument.\n"
                        + "> - Backslashes escape the following character to not receive any special handling.\n"
                        + "> - To use quotes or slashes as literal values anywhere they have to be escaped `\\\"`, "
                        + "will be parsed as `\"`."
                    )
            ),
            new Page().WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Parsing Examples")
                    .WithFooter("5/5")
                    .WithDescription(
                        "> `tag new <name> [content]` can be used to create a tag like this:\n"
                        + ">`tag name with spaces` => `benor haha`.\n"
                        + Markdown.CodeBlock("sbu tag new \"tag name with spaces\" benor haha")
                        + "> To allow quotes in the value name itself, create the tag like this:\n"
                        + Markdown.CodeBlock("sbu tag \\\"\\\"\\\"them\\\"\\\"\\\" ||da jews||\ntag \"\"\"them\"\"\"")
                    )
            )
        );

        [Group("command", "commands")]
        public sealed class CommandGroup : SbuModuleBase

        {
            [Command("find")]
            public DiscordCommandResult Find(string command)
            {
                IEnumerable<Command> matches = Context.Bot.Commands.FindCommands(command).Select(m => m.Command);

                if (!matches.Any())
                    return Reply("Couldn't find any commands for that input");

                // TODO: fill out pages
                return Pages(
                    new Page().WithEmbeds(
                        new LocalEmbed().WithTitle("Command List")
                            .WithDescription(
                                string.Join(
                                    "\n",
                                    matches.Select(
                                        cmd => cmd.IsEnabled ? $"`{cmd.GetSignature()}`" : $"~~`{cmd.GetSignature()}`~~"
                                    )
                                )
                            )
                    )
                );
            }

            [Command("list")]
            public async ValueTask<DiscordCommandResult> ListAsync()
            {
                IEnumerable<Command> commands = Context.Bot.Commands.GetAllCommands();

                if (!Context.Author.GetGuildPermissions().Administrator)
                    commands = await filterOutFailedChecks(commands, Context).CollectAsync();

                // TODO: fill out pages
                return Pages(
                    new Page().WithEmbeds(
                        new LocalEmbed().WithTitle("Command List")
                            .WithDescription(
                                string.Join(
                                    "\n",
                                    commands.Select(
                                        cmd => cmd.IsEnabled ? $"`{cmd.GetSignature()}`" : $"~~`{cmd.GetSignature()}`~~"
                                    )
                                )
                            )
                    )
                );

                static async IAsyncEnumerable<Command> filterOutFailedChecks(
                    IEnumerable<Command> source,
                    DiscordGuildCommandContext context
                )
                {
                    foreach (var cmd in source)
                    {
                        if (await cmd.RunChecksAsync(context) is { IsSuccessful: true })
                            yield return cmd;
                    }
                }
            }
        }

        [Command("help", "h", "how")]
        public DiscordCommandResult Help([OverrideDefault("show all commands")] string? command = null)
        {
            if (command is null)
            {
                Context.RepostAsAlias("command list");
                return null!;
            }

            IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(command);

            // TODO: create proper handling for commands
            if (matches.Count == 0)
                return Reply("No commands found.");

            // TODO: fill out pages
            return Pages(
                new Page().WithEmbeds(
                    new LocalEmbed().WithDescription(
                        string.Join(
                            "\n",
                            matches.Select(c => c.Command)
                                .Select(
                                    cmd =>
                                    {
                                        StringBuilder builder = new();

                                        builder.Append(cmd.IsEnabled ? "`" : "~~`")
                                            .Append(cmd.GetSignature())
                                            .AppendLine(cmd.IsEnabled ? "`" : "`~~");

                                        if (!cmd.IsEnabled)
                                            return builder.ToString();

                                        if (cmd.Description is { })
                                            builder.AppendLine("Description:").AppendLine(cmd.Description);

                                        if (cmd.Remarks is { })
                                            builder.AppendLine("Remarks:").AppendLine(cmd.Remarks);

                                        return builder.ToString();
                                    }
                                )
                        )
                    )
                )
            );
        }
    }
}