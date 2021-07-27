using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Kkommon.Exceptions;

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
            code = evalCleanUp(code);
            CompilationResult compilationResult = Eval.Compile(code, Context);

            switch (compilationResult)
            {
                case CompilationResult.Failed failed:
                    await Reply(failed.GetDiagnosticEmbed());
                    return;

                case CompilationResult.Completed completed:
                {
                    switch (await completed.RunAsync())
                    {
                        case ScriptResult.Failed failedScript:
                            await Reply(failedScript.GetDiagnosticEmbed());
                            return;

                        case ScriptResult.Completed { ReturnValue: { } } completedScript:
                            await Reply(completedScript.GetResultEmbed());
                            return;

                        default:
                            await Context.Message.AddReactionAsync(
                                LocalEmoji.Custom(SbuGlobals.Guild.Emote.Menu.CONFIRM)
                            );

                            return;
                    }
                }

                default:
                    throw new UnreachableException();
            }

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