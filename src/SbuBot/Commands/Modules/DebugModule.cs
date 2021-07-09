using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Evaluation;
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
            [Usage("echo", "echo echo", "echo blah blah blah")]
            public DiscordCommandResult Echo(
                [Description("The message to reply with.")]
                string message = "echo!"
            ) => Reply(message);

            [Command]
            [RequireBotOwner]
            [Description(
                "Removes the original message and replies with the given message in the given target channel."
            )]
            public async Task EchoAsync(
                [Description("The target channel in which to send the message in.")]
                ITextChannel target,
                [Description("The message to reply with.")]
                string message = "echo!"
            )
            {
                await Context.Message.DeleteAsync();
                await target.SendMessageAsync(new LocalMessage().WithContent(message));
            }
        }

        [Command("ping")]
        [Description("Replies with `Pong!` after the given timespan or instantly if no timespan is specified.")]
        [Usage("ping", "ping in 3 seconds")]
        public DiscordCommandResult Send(
            [OverrideDefault("{now}")][Description("The timestamp at which to send the reply.")]
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
                timespan.Value - DateTime.Now
            );

            return Reply($"Scheduled pong to be sent in `{timespan}`.");
        }

        [Group("eval")]
        [RequireBotOwner]
        [Description("Compiles and runs a C#-Script and returns the script result.")]
        [Remarks("Code may be given as plain code or as a code block with `cs` or `csharp` language.")]
        public class EvalGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> EvalMessageAsync(
                [OverrideDefault("{@reply}")][Description("A message containing the code to evaluate.")]
                IUserMessage? codeMessage = null
            )
            {
                codeMessage ??= Context.Message.ReferencedMessage.HasValue
                    ? Context.Message.ReferencedMessage.Value
                    : null;

                if (codeMessage is null)
                    return Reply("No expression or message reference given.");

                string code = Context.Message.ReferencedMessage.Value.Content;
                int index = code.IndexOf("```", StringComparison.OrdinalIgnoreCase);

                if (index != -1)
                    code = code[index..];

                return await EvalAsync(code);
            }

            [Command]
            public async Task<DiscordCommandResult> EvalAsync([Description("The code to evaluate.")] string code)
            {
                code = cleanUp(code);

                List<LocalEmbed> embeds = new(2);
                CompilationResult compilationResult = Eval.Compile(code, Context);

                if (compilationResult is CompilationResult.Completed { Diagnostics: { Count: > 0 } }
                    or CompilationResult.Failed)
                    embeds.Add(compilationResult.ToEmbed());

                if (compilationResult is CompilationResult.Completed completed)
                {
                    ScriptResult result = await completed.RunAsync();
                    embeds.Add(result.ToEmbed());
                }

                return Reply(embeds.ToArray());

                static string cleanUp(string expression)
                {
                    if (!expression.StartsWith("```"))
                        return expression;

                    ReadOnlySpan<char> span = expression.AsSpan();

                    span = span[3..^3];

                    if (span.StartsWith("cs".AsSpan()))
                        span = span[2..];

                    if (span.StartsWith("harp".AsSpan()))
                        span = span[4..];

                    return span.ToString();
                }
            }
        }

        [Group("do")]
        [RequireBotOwner]
        [Description("A group of commands that invoke other commands with a proxy context.")]
        public sealed class ProxySubModule : SbuModuleBase
        {
            [Command]
            [Description(
                "Sends a given proxy command, or `ping` if not specified, as a given author in a given channel."
            )]
            public void Do(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            ) => Context.Bot.Queue.Post(
                new DiscordGuildCommandContext(
                    Context.Bot,
                    Context.Prefix,
                    command,
                    new ProxyMessage(Context.Message, command, member, channel.Id),
                    (channel as CachedTextChannel) ?? Context.Channel,
                    Context.Services
                ),
                context => context.Bot.ExecuteAsync(context)
            );

            [Command("as")]
            [Description("Sends a given proxy command, or `ping` if not specified, as a given author.")]
            public void DoAsUser(
                [Description("The proxy author.")] IMember member,
                [Description("The proxy command.")] string command = "ping"
            ) => Do(member, Context.Channel, command);

            [Command("in")]
            [Description("Sends a given proxy command, or `ping` if not specified, in a given channel.")]
            public void DoInChannel(
                [Description("The proxy channel.")] ITextChannel channel,
                [Description("The proxy command.")] string command = "ping"
            ) => Do(Context.Author, channel, command);
        }

        [Command("lock")]
        [RequireBotOwner]
        [Description("Sets the bot lock state to the given state, or switches it if no state is specified.")]
        public DiscordCommandResult Lock(
            [OverrideDefault("{!state}")][Description("THe new lock state to set the bot to.")]
            bool? set = null
        )
        {
            SbuBot bot = (Context.Bot as SbuBot)!;
            bot.IsLocked = set ?? !bot.IsLocked;
            return Reply($"{(bot.IsLocked ? "Locked" : "Unlocked")} the bot.");
        }

        [Command("test")]
        [RequireBotOwner]
        [Description("Sets the bot lock state to the given state, or switches it if no state is specified.")]
        public DiscordCommandResult Test()
        {
            //return FilledPages(Enumerable.Range(1, 9).Select(i => i.ToString()), 3);
            return HelpView(Context.Bot.Commands.GetAllCommands().First(c => c.Aliases.Contains("as")));
        }
    }
}