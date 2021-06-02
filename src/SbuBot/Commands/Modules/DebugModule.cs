using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed class DebugModule : SbuModuleBase
    {
        [Group("echo")]
        public sealed class EchoGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> EchoAsync(string content)
            {
                await Context.Message.DeleteAsync();
                return Reply(content);
            }

            [Command, RequireBotOwner]
            public async Task EchoAsync(ITextChannel target, string content)
            {
                await Context.Message.DeleteAsync();
                await target.SendMessageAsync(new LocalMessage().WithContent(content));
            }
        }

        [Command("ping")]
        public DiscordCommandResult Send(TimeSpan? timespan = null)
        {
            if (timespan is null)
                return Reply("Pong!");

            SchedulerService service = Context.Services.GetRequiredService<SchedulerService>();

            // Context.Yield()/BeginYield() with delay could be used here but this is for testing the scheduler service
            service.Schedule(
                _ => Context.Channel.SendMessageAsync(
                    new LocalMessage().WithContent($"Ping was scheduled at: `{DateTime.Now}`, Pong!")
                ),
                timespan.Value
            );

            return Reply($"Scheduled pong to be sent in `{timespan}`.");
        }

        [Command("eval"), RequireBotOwner]
        public async Task<DiscordCommandResult> EvalAsync([Remainder] string expression)
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

                    switch (scriptResult)
                    {
                        case ScriptResult.Completed scriptCompleted:
                            return Reply(scriptCompleted.ReturnValue?.ToString() ?? "<null>");

                        case ScriptResult.Failed scriptFailed:
                            return Reply(scriptFailed.Exception.ToString());

                        default:
                            throw new ArgumentOutOfRangeException(nameof(scriptResult), scriptResult, null);
                    }
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

        [Group("do"), RequireBotOwner]
        public sealed class ProxyGroup : SbuModuleBase
        {
            [Command]
            [Description("Send a proxy command as a different author in a different channel.")]
            public void DoAsync(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy message.")] string command = "ping"
            )
            {
                Context.Bot.Queue.Post(
                    new SbuCommandContext(
                        Context.Bot,
                        Context.Prefix,
                        command,
                        new ProxyMessage(Context.Message, command, member, channel.Id),
                        Context.Channel,
                        Context.Services,
                        Context.Invoker
                    ),
                    context => context.Bot.ExecuteAsync(context)
                );
            }

            [Command("as")]
            [Description("Send a proxy command as a different author.")]
            public void DoAsUserAsync(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy command.")] string command = "ping"
            )
            {
                Context.Bot.Queue.Post(
                    new SbuCommandContext(
                        Context.Bot,
                        Context.Prefix,
                        command,
                        new ProxyMessage(Context.Message, command, member),
                        Context.Channel,
                        Context.Services,
                        Context.Invoker
                    ),
                    context => context.Bot.ExecuteAsync(context)
                );
            }

            [Command("in")]
            [Description("Send a proxy command in a different channel.")]
            public void DoInChannelAsync(
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            )
            {
                Context.Bot.Queue.Post(
                    new SbuCommandContext(
                        Context.Bot,
                        Context.Prefix,
                        command,
                        new ProxyMessage(Context.Message, command, proxyChannelId: channel.Id),
                        Context.Channel,
                        Context.Services,
                        Context.Invoker
                    ),
                    context => context.Bot.ExecuteAsync(context)
                );
            }
        }
    }
}