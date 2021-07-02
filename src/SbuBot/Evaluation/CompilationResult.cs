using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Disqord;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace SbuBot.Evaluation
{
    public abstract class CompilationResult
    {
        public TimeSpan CompilationTime { get; }
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        private CompilationResult(TimeSpan compilationTime, IReadOnlyList<Diagnostic> diagnostics)
        {
            CompilationTime = compilationTime;
            Diagnostics = diagnostics;
        }

        public virtual LocalEmbed ToEmbed()
        {
            string description = string.Join(
                "\n",
                Diagnostics.Select(d => $"{SbuGlobals.BULLET} {d.Id}: {d.GetMessage()}")
            );

            return new LocalEmbed()
                .WithFooter(@$"{CompilationTime:s\.ffff\s}")
                .WithDescription(
                    string.Format(
                        "**Diagnostics:**\n{0}",
                        string.IsNullOrWhiteSpace(description) ? "None" : Markdown.CodeBlock("yml", description)
                    )
                );
        }

        public sealed class Completed : CompilationResult
        {
            private readonly Script<object> _script;
            private readonly ScriptGlobals _globalsInstance;

            public Completed(
                Script<object> script,
                ScriptGlobals globalsInstance,
                IReadOnlyList<Diagnostic> diagnostics,
                TimeSpan compilationTime
            ) : base(compilationTime, diagnostics)
            {
                _script = script;
                _globalsInstance = globalsInstance;
            }

            public async Task<ScriptResult> RunAsync()
            {
                var sw = Stopwatch.StartNew();
                ScriptState<object> res = await _script.RunAsync(_globalsInstance, _ => true);
                sw.Stop();

                if (res.Exception is null)
                    return new ScriptResult.Completed(sw.Elapsed, res.ReturnValue);

                return new ScriptResult.Failed(sw.Elapsed, res.Exception);
            }

            public override LocalEmbed ToEmbed() => base.ToEmbed()
                .WithTitle("Compilation completed")
                .WithColor(Color.Green);
        }

        public sealed class Failed : CompilationResult
        {
            public Failed(IReadOnlyList<Diagnostic> diagnostics, TimeSpan compilationTime)
                : base(compilationTime, diagnostics) { }

            public override LocalEmbed ToEmbed() => base.ToEmbed()
                .WithTitle("Compilation failed")
                .WithColor(Color.Red);
        }
    }
}