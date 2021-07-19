using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

using SbuBot.Evaluation;

namespace SbuBot.Commands.Modules
{
    public sealed partial class DebugModule
    {
        [Command("eval")]
        [Description("Compiles and runs a C#-Script.")]
        [Remarks("Code may be given as plain code or as a code block with `cs` or `csharp` language.")]
        public async Task EvalAsync([Description("The code to run.")] string code)
        {
            LocalMessage reply = new();

            code = _evalCleanUp(code);
            await _compileAndRunScriptAsync(code, reply);

            if (reply.Embeds.Count != 0)
                await Reply(reply);
            else
                await Context.Message.AddReactionAsync(LocalEmoji.Custom(SbuGlobals.Emote.Menu.CONFIRM));
        }

        [Command("inspect")]
        [Description("Returns an inspection for the given expression's return value.")]
        public async Task<DiscordCommandResult> Inspect(
            [Description("The expression to inspect.")]
            string expression
        )
        {
            LocalMessage reply = new();

            expression = _evalCleanUp(expression);
            await _compileAndRunScriptAsync($"return {expression};", reply, true);
            return Reply(reply);
        }

        private static string _evalCleanUp(string expression)
        {
            ReadOnlySpan<char> span = expression.AsSpan();

            if (!expression.StartsWith('`'))
                return expression;

            span = span[1..^1];

            if (!span.StartsWith("``"))
                return span.ToString();

            span = span[2..^2];

            if (span.StartsWith("cs".AsSpan()))
                span = span[2..];

            if (span.StartsWith("harp".AsSpan()))
                span = span[4..];

            return span.ToString();
        }

        private async Task _compileAndRunScriptAsync(string code, LocalMessage reply, bool inspect = false)
        {
            CompilationResult compilationResult = Eval.Compile(code, Context);

            switch (compilationResult)
            {
                case CompilationResult.Failed failed:
                    reply.AddEmbed(failed.ToEmbed());
                    return;

                case CompilationResult.Completed { Diagnostics: { Count: > 0 } } completed:
                    reply.AddEmbed(completed.ToEmbed());
                    break;

                case CompilationResult.Completed completed:
                {
                    switch (await completed.RunAsync())
                    {
                        case ScriptResult.Failed failedScript:
                            reply.AddEmbed(failedScript.ToEmbed());
                            break;

                        case ScriptResult.Completed completedScript when inspect:
                            reply.AddEmbed(completedScript.ToEmbed());
                            break;
                    }

                    break;
                }
            }
        }
    }
}