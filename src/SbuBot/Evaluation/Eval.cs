using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Disqord.Bot;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace SbuBot.Evaluation
{
    public static class Eval
    {
        public static readonly string[] IMPORTS =
        {
            "System",
            "System.Linq",
            "System.Threading",
            "Disqord",
            "Disqord.Bot",
            "Disqord.Gateway",
            "Disqord.Rest",
        };

        public static readonly Assembly[] REFERENCES = Assembly.GetCallingAssembly()
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .ToArray();

        public static readonly ScriptOptions SCRIPT_OPTIONS = ScriptOptions.Default
            .WithImports(Eval.IMPORTS)
            .WithAllowUnsafe(true)
            .WithReferences(Eval.REFERENCES);

        public static CompilationResult CreateAndCompile(string code, DiscordGuildCommandContext context)
        {
            Script<object> script = CSharpScript.Create(code, Eval.SCRIPT_OPTIONS, typeof(ScriptGlobals));

            var sw = Stopwatch.StartNew();
            ImmutableArray<Diagnostic> diagnostics = script.Compile();
            sw.Stop();

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                return new CompilationResult.Failed(diagnostics, sw.Elapsed);

            return new CompilationResult.Completed(script, new ScriptGlobals(context), diagnostics, sw.Elapsed);
        }
    }
}