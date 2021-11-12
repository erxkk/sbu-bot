using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;

using Qmmands;

using SbuBot.Evaluation;

namespace SbuBot.Commands.Modules
{
    [RequireBotOwner]
    [Description("A collection of commands for debugging and testing.")]
    public sealed partial class DebugModule : SbuModuleBase
    {
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

        [Command("chunk")]
        [Description("Big Big, Chunkus, Big Chunkus, Big Chunkus.")]
        public async Task<DiscordCommandResult> ChunkAsync()
        {
            await using (Context.BeginYield())
            {
                await Context.Bot.Chunker.ChunkAsync(Context.Guild);
            }

            return Response("Big Chunkus.");
        }

        [Command("kill")]
        [Description("Fucking kills the bot oh my god...")]
        public async Task KillAsync()
        {
            await Response("");
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
