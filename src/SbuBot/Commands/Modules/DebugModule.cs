using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Information;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of commands for debugging and testing.")]
    public sealed class DebugModule : SbuModuleBase
    {
        [Group("echo")]
        [Description("A group of commands for testing the bots channel access.")]
        public sealed class EchoGroup : SbuModuleBase
        {
            [Command]
            [Description("Replies with the given message.")]
            public async Task<DiscordCommandResult> EchoAsync(
                [Description("The message to reply with.")]
                string message
            ) => Reply(message);

            [Command, RequireBotOwner]
            [Description(
                "Removes the original message and replies with the given message in the given target channel."
            )]
            public async Task EchoAsync(
                [Description("The target channel in which to send the message in.")]
                ITextChannel target,
                [Description("The message to reply with.")]
                string message
            )
            {
                await Context.Message.DeleteAsync();
                await target.SendMessageAsync(new LocalMessage().WithContent(message));
            }
        }

        [Command("ping")]
        [Description("Replies with `Pong!` after the given timespan or instantly if no timespan is specified.")]
        public DiscordCommandResult Send(
            [OverrideDefault("now")][Description("The timestamp at which to send the reply.")]
            DateTime? timespan = null
        )
        {
            if (timespan is null)
                return Reply("Pong!");

            SchedulerService service = Context.Services.GetRequiredService<SchedulerService>();

            // Context.Yield()/BeginYield() with delay could be used here but this is for testing the scheduler service
            service.Schedule(
                _ => Context.Channel.SendMessageAsync(
                    new LocalMessage().WithContent($"Ping was scheduled at: `{DateTime.Now}`, Pong!")
                ),
                timespan.Value - DateTime.Now,
                0,
                Context.Bot.StoppingToken
            );

            return Reply($"Scheduled pong to be sent in `{timespan}`.");
        }

        [Command("eval"), RequireBotOwner]
        [Description("Compiles and runs a C#-Script and returns the script result.")]
        public async Task<DiscordCommandResult> EvalAsync(
            [Description("The expression to evaluate.")]
            string expression
        )
        {
            EvalService service = Context.Services.GetRequiredService<EvalService>();
            DateTimeOffset startTime = DateTimeOffset.Now;
            CompilationResult compilationResult = service.CreateAndCompile(expression, Context);

            // TODO: refine to include compilation time etc
            switch (compilationResult)
            {
                case CompilationResult.Completed compilationCompleted:
                {
                    ScriptResult scriptResult = await compilationCompleted.RunAsync();

                    return scriptResult switch
                    {
                        ScriptResult.Completed scriptCompleted => Reply(
                            scriptCompleted.ReturnValue?.ToString() ?? "<null>"
                        ),
                        ScriptResult.Failed scriptFailed => Reply(scriptFailed.Exception.ToString()),
                        _ => throw new ArgumentOutOfRangeException(nameof(scriptResult), scriptResult, null),
                    };
                }

                case CompilationResult.Failed compilationFailed:
                {
                    return MaybePages(
                        compilationFailed.Diagnostics.Select(d => d.Id),
                        "Compilation failed",
                        "Failed at",
                        startTime + compilationFailed.CompilationTime
                    );
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(compilationResult), compilationResult, null);
            }
        }

        // currently not before/after execute is done etc
        [Group("do"), RequireBotOwner]
        [Description("A group of commands that invoke other commands with a proxy context.")]
        public sealed class ProxyGroup : SbuModuleBase
        {
            [Command]
            [Description(
                "Sends a given proxy command, or `ping` if not specified, as a given author in a given channel."
            )]
            public async Task DoAsync(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            )
            {
                var ctx = new SbuCommandContext(
                    Context.Bot,
                    Context.Prefix,
                    command,
                    new ProxyMessage(Context.Message, command, member, channel.Id),
                    Context.Channel,
                    Context.Services
                );

                await ctx.InitializeAsync();
                Context.Bot.Queue.Post(ctx, context => context.Bot.ExecuteAsync(context));
            }

            [Command("as")]
            [Description("Sends a given proxy command, or `ping` if not specified, as a given author.")]
            public async Task DoAsUserAsync(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy command.")] string command = "ping"
            )
            {
                var ctx = new SbuCommandContext(
                    Context.Bot,
                    Context.Prefix,
                    command,
                    new ProxyMessage(Context.Message, command, member),
                    Context.Channel,
                    Context.Services
                );

                await ctx.InitializeAsync();
                Context.Bot.Queue.Post(ctx, context => context.Bot.ExecuteAsync(context));
            }

            [Command("in")]
            [Description("Sends a given proxy command, or `ping` if not specified, in a given channel.")]
            public async Task DoInChannelAsync(
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            )
            {
                var ctx = new SbuCommandContext(
                    Context.Bot,
                    Context.Prefix,
                    command,
                    new ProxyMessage(Context.Message, command, proxyChannelId: channel.Id),
                    Context.Channel,
                    Context.Services
                );

                await ctx.InitializeAsync();
                Context.Bot.Queue.Post(ctx, context => context.Bot.ExecuteAsync(context));
            }
        }

        [Command("lock")]
        [Description("Sets the bot lock state to the given state, or switches it if no state is specified.")]
        public DiscordCommandResult Lock(
            [OverrideDefault("!state")][Description("THe new lock state to set the bot to.")]
            bool? set = null
        )
        {
            Context.Bot.IsLocked = set ?? !Context.Bot.IsLocked;
            return Reply($"{(Context.Bot.IsLocked ? "Locked" : "Unlocked")} the bot.");
        }
    }
}