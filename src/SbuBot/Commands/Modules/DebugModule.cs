using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Views;
using SbuBot.Evaluation;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    [RequireBotOwner]
    [Description("A collection of commands for debugging and testing.")]
    public sealed partial class DebugModule : SbuModuleBase
    {
        [Command("echo")]
        [Description(
            "Removes the original message and replies with the given message in the given target channel."
        )]
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

        [Group("do")]
        [Description("A group of commands that invoke other commands with a proxy context.")]
        public sealed class ProxySubModule : SbuModuleBase
        {
            [Command]
            [Description(
                "Sends a given proxy command, or `ping` if not specified, as a given author in a given channel."
            )]
            public async Task DoAsync(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy channel.")] IMessageGuildChannel channel,
                [Description("The proxy command.")] string command = "ping"
            )
            {
                // avoid disposal of the current context by waiting until the command completed
                var tcs = new TaskCompletionSource();

                Context.Bot.Queue.Post(
                    new DiscordGuildCommandContext(
                        Context.Bot,
                        Context.Prefix,
                        command,
                        new ProxyMessage(Context.Message, command, member, channel.Id),
                        (channel as CachedTextChannel) ?? Context.Channel,
                        Context.Services
                    ),
                    async context =>
                    {
                        await context.Bot.ExecuteAsync(context);
                        tcs.SetResult();
                    }
                );

                await using (Context.BeginYield())
                {
                    await tcs.Task;
                }
            }

            [Command("as")]
            [Description("Sends a given proxy command, or `ping` if not specified, as a given author.")]
            public Task DoAsUserAsync(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy command.")] string command = "ping"
            ) => DoAsync(member, Context.Channel, command);

            [Command("in")]
            [Description("Sends a given proxy command, or `ping` if not specified, in a given channel.")]
            public Task DoInChannelAsync(
                [Description("The proxy channel.")] IMessageGuildChannel channel,
                [Description("The proxy command.")] string command = "ping"
            ) => DoAsync(Context.Author, channel, command);
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
                        .Where(c => c.FullAliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase)))
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
                        .Where(c => c.FullAliases.Any(a => a.Equals(path, StringComparison.OrdinalIgnoreCase)))
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

        [Command("chunk")]
        [Description("Big Big, Chunkus, Big Chunkus, Big Chunkus.")]
        public async Task<DiscordCommandResult> ChunkAsync()
        {
            await using (Context.BeginYield())
            {
                await Context.Bot.Chunker.ChunkAsync(Context.Guild);
            }

            return Reply("Big Chunkus.");
        }

        [Command("kill")]
        [Description("Fucking kills the bot oh my god...")]
        public async Task KillAsync()
        {
            await Reply("Gn kid.");
            Environment.Exit(0);
        }

        [Command("stat")]
        [Description("Displays process statistics of the bot.")]
        public DiscordCommandResult Stat()
        {
            Process process = Process.GetCurrentProcess();
            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();

            return Reply(
                new LocalEmbed()
                    .WithTitle("Process Stats")
                    .WithDescription(
                        string.Format(
                            @"```

pmem  |      | {0,16:N1}  B
vmem  |      | {1,16:N1}  B
vmem  | peak | {2,16:N1}  B

ws    |      | {3,16:N1}  B
ws    | peak | {4,16:N1}  B

heap  |      | {5,16:N1}  B

ptime |      | {6,16:N1} ms
ptime | priv | {7,16:N1} ms
ptime | user | {8,16:N1} ms
```",
                            process.PrivateMemorySize64,
                            process.VirtualMemorySize64,
                            process.PeakVirtualMemorySize64,
                            process.WorkingSet64,
                            process.PeakWorkingSet64,
                            memoryInfo.HeapSizeBytes,
                            process.TotalProcessorTime.TotalMilliseconds,
                            process.PrivilegedProcessorTime.TotalMilliseconds,
                            process.UserProcessorTime.TotalMilliseconds
                        )
                    )
                    .WithTimestamp(process.StartTime)
            );
        }

        [Command("collect")]
        [Description("Forces garbage collection.")]
        public DiscordCommandResult Collect()
        {
            long preCollection = GC.GetTotalMemory(false);
            GC.Collect();
            long postCollection = GC.GetTotalMemory(false);

            return Reply($"Collected around {preCollection - postCollection:N1} bytes.");
        }

        [Command("parse")]
        [Description("Attempts to parse the given input.")]
        public async Task<DiscordCommandResult> ParseAsync(
            [Description("The type to try parsing the value into.")]
            string type,
            [Description("The value to parse.")] string value,
            [Description("The optional format to apply when printing the results.")]
            string? format = null
        )
        {
            // we never parse strings, so using them as sentinels for failure is fine
            string script = string.Format(
                @"
                    var parser = Context.Bot.Commands.GetTypeParser<{0}>();
                    return parser is {{ }}
                        ? await parser.ParseAsync(null, ""{1}"", Context) is {{ IsSuccessful: true }} result
                            ? (object) result.Value
                            : (object) ""no result""
                        : (object) ""no parser found"";
                ",
                type,
                value
            );

            CompilationResult result = Eval.Compile(script, Context);

            switch (result)
            {
                case CompilationResult.Completed completed:
                {
                    ScriptResult scriptResult = await completed.RunAsync();

                    switch (scriptResult)
                    {
                        case ScriptResult.Completed { ReturnValue: { } } completedScript:
                        {
                            if (completedScript.ReturnValue is string str)
                                return Reply(str);

                            return Reply(
                                new LocalEmbed()
                                    .WithTitle("Parsed Value")
                                    .WithDescription(
                                        string.Format(
                                            $"{{0{(format is null ? "" : $":{format}")}}}",
                                            completedScript.ReturnValue
                                        )
                                    )
                            );
                        }

                        case ScriptResult.Failed failedScript:
                            return Reply(failedScript.GetDiagnosticEmbed());

                        default:
                            throw new ArgumentOutOfRangeException(nameof(scriptResult));
                    }
                }

                case CompilationResult.Failed failed:
                    return Reply(failed.GetDiagnosticEmbed());

                default: throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        [Command("test")]
        [Description("A test command.")]
        public async Task<DiscordCommandResult> TestAsync()
        {
            await Task.Yield();
            return Reply("test");
        }
    }
}
