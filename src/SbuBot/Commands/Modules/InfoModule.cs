using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;

using Kkommon.Extensions.AsyncLinq;

using Qmmands;

using SbuBot.Commands.Information;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    // TODO: annotate when finished
    // TODO: improve syntax explanation, add descriptor explanation
    // TODO: add dynamic menu with navigation when buttons are finished
    // TODO: add informational commands
    [Description("A collection of commands for help and general server/member/bot information.")]
    public sealed class InfoModule : SbuModuleBase
    {
        [Command("about")]
        public DiscordCommandResult About() => Reply(
            new LocalEmbed()
                .WithTitle("Sbu-Bot")
                .WithDescription("Bot for management for the sbu server.")
                .AddInlineField("Default Prefix", SbuGlobals.DEFAULT_PREFIX)
                .AddInlineField("Version", SbuGlobals.VERSION)
                .AddInlineField(
                    "Written in",
                    $"{Markdown.Link("Disqord", "https://github.com/quahu/disqord")} (C#)"
                )
        );

        [Command("guide")]
        public DiscordCommandResult Guide() => Pages(
            new LocalEmbed()
                .WithTitle("Commands")
                .WithFooter("1/5")
                .WithDescription(
                    "To use Commands ping the bot or send a message that starts with "
                    + $"'{SbuGlobals.DEFAULT_PREFIX}', the space between the prefix and the command is "
                    + "optional and does not influence the command execution, both `sbu ping` and "
                    + "`sbuping` will work just fine."
                ),
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
                ),
            new LocalEmbed()
                .WithTitle("Parameters Examples")
                .WithFooter("3/5")
                .WithDescription(
                    "> `ban <user> [reason = \"beaned\"]` can be used like:\n"
                    + Markdown.CodeBlock("sbu ban @joemama\nsbu ban @joemama you're a jew")
                    + "\n> `gift <user> <tag> [additional tags…]` can be used like:\n"
                    + Markdown.CodeBlock("sbu gift @joemama tag1\nsbu gift @joemama tag1 tag2 tag3")
                ),
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
                ),
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

                return MaybePages(
                    matches.Select(cmd => cmd.IsEnabled ? $"`{cmd.GetSignature()}`" : $"~~`{cmd.GetSignature()}`~~"),
                    "Command List"
                );
            }

            [Command("list")]
            public async Task<DiscordCommandResult> ListAsync()
            {
                IEnumerable<Command> enumerable = Context.Bot.Commands.GetAllCommands();

                if (!Context.Author.GetGuildPermissions().Administrator)
                {
                    enumerable = await enumerable
                        .AsyncWhere(async cmd => await cmd.RunChecksAsync(Context) is { IsSuccessful: true })
                        .CollectAsync();
                }

                return MaybePages(
                    enumerable.Select(cmd => cmd.IsEnabled ? $"`{cmd.GetSignature()}`" : $"~~`{cmd.GetSignature()}`~~"),
                    "Command List"
                );
            }
        }

        [Command("help", "h", "how")]
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
                        Context.Services
                    ),
                    context => context.Bot.ExecuteAsync(context)
                );

                return null!;
            }

            IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(command);

            // TODO: create proper handling for commands
            if (matches.Count == 0)
                return Reply("No commands found.");

            return MaybePages(
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
            );
        }
    }
}