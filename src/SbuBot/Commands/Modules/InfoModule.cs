using System.Collections.Generic;
using System.Linq;
using System.Text;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Information;

namespace SbuBot.Commands.Modules
{
    public sealed class InfoModule : SbuModuleBase
    {
        [Command("reserved")]
        public DiscordCommandResult ReservedKeywords() => Reply(
            "The following keywords are not allowed for tag names, but tags may contain them:\n"
            + string.Join("\n", SbuBotGlobals.RESERVED_NAMES.Select(rn => $"> {rn}"))
        );

        [Command("guide")]
        public DiscordCommandResult Guide() => Pages(
            new LocalEmbedBuilder()
                .WithTitle("Commands")
                .WithFooter("1/5")
                .WithDescription(
                    "To use Commands ping the bot or send a message that starts with "
                    + $"'{SbuBotGlobals.DEFAULT_PREFIX}', the space between the prefix and the command is "
                    + "optional and does not influence the command execution, both `sbu ping` and "
                    + "`sbuping` will work just fine."
                ),
            new LocalEmbedBuilder()
                .WithTitle("Parameters")
                .WithFooter("2/5")
                .WithDescription(
                    "Brackets in help commands indicate parameter importance:"
                    + "\n> - `<param>` indicates a required parameter, it cannot be left out."
                    + "\n> - `[param = default]` indicates an optional parameter, if left out the default value "
                    + "will be used."
                    + "\n> - `[params…]` indicates a collection of values, if left out none will be passed."
                    + "\n> - All parameters but the last of each command are separated by spaces by default."
                ),
            new LocalEmbedBuilder()
                .WithTitle("Parameters Examples")
                .WithFooter("3/5")
                .WithDescription(
                    "> `ban <user> [reason = \"beaned\"]` can be used like:\n"
                    + Markdown.CodeBlock("sbu ban @joemama\nsbu ban @joemama you're a jew")
                    + "\n> `gift <user> <tag> [additional tags…]` can be used like:\n"
                    + Markdown.CodeBlock("sbu gift @joemama tag1\nsbu gift @joemama tag1 tag2 tag3")
                ),
            new LocalEmbedBuilder()
                .WithTitle("Parsing")
                .WithFooter("4/5")
                .WithDescription(
                    "Quotes and backslashes receive special handling when parsing:\n"
                    + "> - Quotes `\"counts as one\"` indicate the start and end of an argument that contains "
                    + "spaces and is not the last argument, they are ignored on the last argument.\n"
                    + "> - Backslashes escape the following character to not receive any special handling.\n"
                    + "> - To use quotes or slashes as literal values anywhere they have to be escaped `\\\"`, "
                    + "will be parsed as `\"`."
                ),
            new LocalEmbedBuilder()
                .WithTitle("Parsing Examples")
                .WithFooter("5/5")
                .WithDescription(
                    "> `tag new <name> [content]` can be used to create a tag like this:\n"
                    + ">`tag name with spaces` => `benor haha`.\n"
                    + Markdown.CodeBlock("sbu tag new \"tag name with spaces\" benor haha")
                    + "> To allow quotes in the value name itself, create the tag like this:\n"
                    + Markdown.CodeBlock("sbu tag \\\"\\\"\\\"them\\\"\\\"\\\" ||da jews||\ntag \"\"\"them\"\"\"")
                )
        );

        [Group("command", "commands", "cmd")]
        public class CommandGroup : SbuModuleBase
        {
            [Command("find")]
            public DiscordCommandResult FindCommand(string command)
            {
                // TODO: interactive?
                IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(command);

                if (!matches.Any())
                    return Reply("Couldn't find any commands for that input");

                return MaybePages(matches.Select(c => c.Command.GetSignature()), "Commands Found");
            }

            [Command("list")]
            public DiscordCommandResult ListCommands()
            {
                return Reply("");
            }
        }

        [Command("help", "h", "how", "howto")]
        public DiscordCommandResult Help([OverrideDefault("show all commands")] string? command = null)
        {
            if (command is null)
            {
                Context.Bot.Queue.Post(
                    new SbuCommandContext(
                        Context.Bot,
                        Context.Prefix,
                        "command list",
                        new ProxyMessage(
                            Context.Message,
                            $"{Context.Prefix} command list",
                            Context.Author,
                            Context.Channel.Id
                        ),
                        Context.Channel,
                        Context.Services,
                        Context.Invoker
                    ),
                    context => context.Bot.ExecuteAsync(context)
                );

                return null!;
            }

            LocalEmbedBuilder embedBuilder = new();
            StringBuilder builder = new();
            IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(command);

            // TODO: handle multiple commands
            if (matches.Count == 0)
                return Reply("No commands found.");

            foreach (Command commandMatch in matches.Select(c => c.Command))
            {
                builder.AppendLine($"`{commandMatch.GetSignature()}`");

                if (commandMatch.Description is { })
                    builder.AppendLine("Description:").AppendLine(commandMatch.Description);

                if (commandMatch.Remarks is { })
                    builder.AppendLine("Remarks:").AppendLine(commandMatch.Remarks);
            }

            return Reply(embedBuilder.WithDescription(builder.ToString()));
        }
    }
}