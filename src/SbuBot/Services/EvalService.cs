using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Bot.Hosting;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

using SbuBot.Commands;
using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class EvalService : DiscordBotService
    {
        public EvalService(ILogger<EvalService> logger, DiscordBotBase bot) : base(logger, bot) { }

        public CompilationResult CreateAndCompile(string code, SbuCommandContext context)
        {
            Script<object> script = CSharpScript.Create(code, ScriptOptions.Default, typeof(ScriptGlobals));

            var sw = Stopwatch.StartNew();
            ImmutableArray<Diagnostic> diagnostics = script.Compile();
            sw.Stop();

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                return new CompilationResult.Failed(diagnostics, sw.Elapsed);

            return new CompilationResult.Completed(script, new(context), diagnostics, sw.Elapsed);
        }
    }

    public sealed class ScriptGlobals
    {
        public SbuCommandContext Context { get; }
        public ScriptGlobals(SbuCommandContext context) => Context = context;
    }

    public abstract class CompilationResult
    {
        public TimeSpan CompilationTime { get; }
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        private CompilationResult(TimeSpan compilationTime, IReadOnlyList<Diagnostic> diagnostics)
        {
            CompilationTime = compilationTime;
            Diagnostics = diagnostics;
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
        }

        public sealed class Failed : CompilationResult
        {
            public Failed(IReadOnlyList<Diagnostic> diagnostics, TimeSpan compilationTime)
                : base(compilationTime, diagnostics) { }
        }
    }

    public abstract class ScriptResult
    {
        public TimeSpan CompletionTime { get; protected init; }

        public sealed class Completed : ScriptResult
        {
            public object? ReturnValue { get; }

            public Completed(TimeSpan completionTime, object? returnValue)
            {
                CompletionTime = completionTime;
                ReturnValue = returnValue;
            }
        }

        public sealed class Failed : ScriptResult
        {
            public Exception Exception { get; }

            public Failed(TimeSpan completionTime, Exception exception)
            {
                CompletionTime = completionTime;
                Exception = exception;
            }
        }
    }
}