using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Views;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    [RequireBotOwner]
    [Description("A collection of bot owner only commands.")]
    public sealed class OwnerModule : SbuModuleBase
    {
        [Command("echo")]
        [Description("Removes the original message and replies with the given message in the given target channel.")]
        public async Task EchoAsync(
            [Description("The target channel in which to send the message in.")]
            IMessageGuildChannel channel,
            [Description("The message to reply with.")]
            string message = "echo!"
        )
        {
            await Context.Message.DeleteAsync();
            await channel.SendMessageAsync(new LocalMessage().WithContent(message));
        }

        [Command("lock")]
        [Description("Sets the bot lock state to the given state, or switches it if no state is specified.")]
        public DiscordCommandResult Lock(
            [OverrideDefault("{!state}")][Description("The new lock state to set the bot to.")]
            bool? set = null
        )
        {
            SbuBot bot = (Context.Bot as SbuBot)!;
            bot.IsLocked = set ?? !bot.IsLocked;
            return Reply($"{(bot.IsLocked ? "Locked" : "Unlocked")} the bot.");
        }

        [Command("toggle")]
        [Description("Disables/Enables a given command or module.")]
        [Remarks(
            "The query can be prefixed with `command:`/`module:` to further specify the query for ambiguous paths."
        )]
        public async Task<DiscordCommandResult> ToggleAsync(
            [Description("The command or module to disable/enable.")]
            string query
        )
        {
            string[] parts = query.Split(':');

            (string? specification, string path) = parts.Length == 2
                ? (parts[0].Trim(), parts[1].Trim())
                : (null, query);

            object commandOrModule;

            switch (specification)
            {
                case "command" or "c":
                {
                    IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(path);

                    switch (matches.Count)
                    {
                        case 0:
                            return Reply($"No command path matches `{path}`.");

                        case 1:
                            commandOrModule = matches[0].Command;
                            break;

                        default:
                        {
                            return Reply(
                                new LocalEmbed()
                                    .WithTitle("Multiple command matches found")
                                    .WithDescription(
                                        string.Format(
                                            "Path:\n{0}\nCommands:\n{1}",
                                            path,
                                            matches.Select(m => $"{SbuGlobals.BULLET} {m.Command.Format()}")
                                                .ToNewLines()
                                        )
                                    )
                            );
                        }
                    }

                    break;
                }

                case "module" or "m":
                {
                    Module[] moduleMatches = Context.Bot.Commands.GetAllModules()
                        .Where(
                            mod => mod.FullAliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase))
                                || mod.Name.Equals(path, StringComparison.OrdinalIgnoreCase)
                        )
                        .ToArray();

                    switch (moduleMatches.Length)
                    {
                        case 0:
                            return Reply($"No module path matches `{path}`.");

                        case 1:
                            commandOrModule = moduleMatches[0];
                            break;

                        default:
                        {
                            return Reply(
                                new LocalEmbed()
                                    .WithTitle("Multiple module matches found")
                                    .WithDescription(
                                        string.Format(
                                            "Path:\n{0}\nModules:\n{1}",
                                            path,
                                            moduleMatches.Select(m => $"{SbuGlobals.BULLET} {m.Format()}")
                                                .ToNewLines()
                                        )
                                    )
                            );
                        }
                    }

                    break;
                }

                case null:
                {
                    IReadOnlyList<CommandMatch> matches = Context.Bot.Commands.FindCommands(query);

                    Module[] moduleMatches = Context.Bot.Commands.GetAllModules()
                        .Where(
                            mod => mod.FullAliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase))
                                || mod.Name.Equals(path, StringComparison.OrdinalIgnoreCase)
                        )
                        .ToArray();

                    switch ((matches.Count, moduleMatches.Length))
                    {
                        case (0, 0):
                            return Reply("No command or module match found.");

                        case (1, 0):
                            commandOrModule = matches[0].Command;
                            break;

                        case (0, 1):
                            commandOrModule = moduleMatches[0];
                            break;

                        default:
                        {
                            return Reply(
                                new LocalEmbed()
                                    .WithTitle("Multiple command and module matches found")
                                    .WithDescription(
                                        string.Format(
                                            "Path:\n{0}\nCommands:\n{1}\nModules:\n{2}",
                                            path,
                                            matches.Select(m => $"{SbuGlobals.BULLET} {m.Command.Format()}")
                                                .ToNewLines(),
                                            moduleMatches.Select(m => $"{SbuGlobals.BULLET} {m.Format()}")
                                                .ToNewLines()
                                        )
                                    )
                            );
                        }
                    }

                    break;
                }

                case var other:
                    return Reply($"Unknown specifier `{other}`.");
            }

            (string type, bool isEnabled, string alias, Action action) = commandOrModule switch
            {
                Command command => ("command", command.IsEnabled, command.FullAliases[0],
                    (Action)(() =>
                    {
                        if (command.IsEnabled)
                            command.Disable();
                        else
                            command.Enable();
                    })),

                Module module => ("module", module.IsEnabled, module.FullAliases[0],
                    (() =>
                    {
                        if (module.IsEnabled)
                            module.Disable();
                        else
                            module.Enable();
                    })),

                _ => throw new ArgumentOutOfRangeException(),
            };

            ConfirmationState result = await ConfirmationAsync(
                $"{(isEnabled ? "Disable" : "Enable")} this {type}?",
                $"You're about to {(isEnabled ? "disable" : "enable")} `{alias}`, proceed?"
            );

            switch (result)
            {
                case ConfirmationState.None:
                case ConfirmationState.TimedOut:
                case ConfirmationState.Aborted:
                    return Reply("Aborted.");

                case ConfirmationState.Confirmed:
                    action();
                    return Reply($"{(isEnabled ? "Disabled" : "Enabled")} `{alias}`.");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Group("status")]
        [Description("A group of commands for setting the bot status.")]
        public class StatusGroup : SbuModuleBase
        {
            [Command]
            [Description("Set the epic bot status.")]
            public async Task SetStatusAsync(
                [Description("The activity type to use.")]
                ActivityType type,
                [Description("The value to set it to.")]
                string value
            )
            {
                // it seems like if streaming generally needs a link so we ignore that as well
                if (type is ActivityType.Custom or ActivityType.Streaming)
                {
                    await Reply("That ain't gonna work chief.");
                    return;
                }

                await Context.Bot.SetPresenceAsync(new LocalActivity(value, type));
            }

            [Command("unset")]
            [Description("Set the epic bot status.")]
            public Task UnsetStatusAsync()
                => Context.Bot.SetPresenceAsync(null);
        }
    }
}
