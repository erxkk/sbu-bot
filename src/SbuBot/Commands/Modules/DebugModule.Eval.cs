using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Evaluation;

namespace SbuBot.Commands.Modules
{
    public sealed partial class DebugModule
    {
        [Command("eval")]
        [Description("Compiles and runs a C#-Script.")]
        [Remarks("Code may be given as plain code or as a code block with `cs` or `csharp` language.")]
        public async Task<DiscordCommandResult> EvalAsync([Description("The code to run.")] string code)
        {
            code = evalCleanUp(code);
            CompilationResult compilationResult = Eval.Compile(code, Context);

            return compilationResult switch
            {
                CompilationResult.Failed failed
                    => Reply(failed.GetDiagnosticEmbed()),
                CompilationResult.Completed completed
                    => await completed.RunAsync() switch
                    {
                        ScriptResult.Failed failedScript
                            => Reply(failedScript.GetDiagnosticEmbed()),

                        ScriptResult.Completed { ReturnValue: { } } completedScript
                            => Reply(completedScript.GetResultEmbed()),

                        _ => Reaction(LocalEmoji.Custom(SbuGlobals.Emote.Menu.CONFIRM)),
                    },

                _ => throw new ArgumentOutOfRangeException(),
            };

            static string evalCleanUp(string expression)
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
        }
    }
}