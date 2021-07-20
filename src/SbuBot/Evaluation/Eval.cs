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
            "Microsoft.EntityFrameworkCore",
            "Disqord",
            "Disqord.Bot",
            "Disqord.Gateway",
            "Disqord.Rest",
            "SbuBot",
            "SbuBot.Extensions",
            "SbuBot.Evaluation.Inspection",
            "SbuBot.Models",
            "SbuBot.Services",
        };

        public static readonly Assembly[] REFERENCES = typeof(Eval).Assembly
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Append(typeof(Eval).Assembly)
            .ToArray();

        public static readonly ScriptOptions SCRIPT_OPTIONS = ScriptOptions.Default
            .WithImports(IMPORTS)
            .WithAllowUnsafe(true)
            .WithReferences(REFERENCES);

        public static CompilationResult Compile(string code, DiscordGuildCommandContext context)
        {
            Script<object> script = CSharpScript.Create(code, SCRIPT_OPTIONS, typeof(ScriptGlobals));

            var sw = Stopwatch.StartNew();
            ImmutableArray<Diagnostic> diagnostics = script.Compile();
            sw.Stop();

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                return new CompilationResult.Failed(diagnostics, sw.Elapsed);

            return new CompilationResult.Completed(script, new(context), diagnostics, sw.Elapsed);
        }
    }
}