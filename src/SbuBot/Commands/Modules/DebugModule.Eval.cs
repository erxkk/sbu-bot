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
        public Task<DiscordCommandResult> EvalAsync([Description("The code to run.")] string code)
            => EvalAsync(1, 2, 5, code);

        [Command("eval")]
        [Description("Compiles and runs a C#-Script with the given inspection parameters.")]
        [Remarks("Code may be given as plain code or as a code block with `cs` or `csharp` language.")]
        public async Task<DiscordCommandResult> EvalAsync(
            [Description("The max depth to inspect to.")][Remarks("Passing null uses default of 1.")]
            int? depth,
            [Description("The indentation delta.")][Remarks("Passing null uses default of 2.")]
            int? delta,
            [Description("The max element count to display.")][Remarks("Passing null uses default of 5.")]
            int? count,
            [Description("The code to run.")] string code
        )
        {
            depth ??= 1;
            delta ??= 2;
            count ??= 5;
            code = DebugModule.EvalCleanUp(code);
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
                            => Inspection(completedScript, depth.Value, delta.Value, count.Value),

                        _ => Reaction(LocalEmoji.Custom(SbuGlobals.Emote.Menu.CONFIRM)),
                    },

                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private static string EvalCleanUp(string expression)
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

        private DiscordCommandResult Inspection(
            ScriptResult.Completed result,
            int maxDepth = 1,
            int indentationDelta = 2,
            int itemCount = 5
        )
        {
            // doesn't throw on null no check needed
            string inspection = result.ReturnValue.GetInspection(maxDepth, indentationDelta, itemCount);

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