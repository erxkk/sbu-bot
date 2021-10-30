using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

using Kkommon.Extensions.Enumerable;

using Qmmands;

using SbuBot.Evaluation;
using SbuBot.Extensions;

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
                            => Inspection(completedScript),

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

        private DiscordCommandResult Inspection(ScriptResult.Completed result, int maxDepth = 2)
        {
            // doesn't throw on null no check needed
            string inspection = result.ReturnValue.GetInspection(maxDepth);

            if (inspection.Length > 2048 + 1024)
            {
                return Pages(
                    new ListPageProvider(
                        inspection.Chunk(2048)
                            .Select(
                                chunk => new Page().WithEmbeds(
                                    new LocalEmbed().WithDescription(Markdown.CodeBlock("yml", new(chunk)))
                                        .WithFooter(@$"{result.CompletionTime:s\.ffff\s}")
                                )
                            )
                    )
                );
            }

            return Response(new LocalEmbed().WithDescription(Markdown.CodeBlock("yml", inspection)));
        }
    }
}