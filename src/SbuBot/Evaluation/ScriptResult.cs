using System;

using Disqord;

namespace SbuBot.Evaluation
{
    public abstract class ScriptResult
    {
        public TimeSpan CompletionTime { get; }

        private ScriptResult(TimeSpan completionTime) => CompletionTime = completionTime;

        public sealed class Completed : ScriptResult
        {
            public object? ReturnValue { get; }

            public Completed(TimeSpan completionTime, object? returnValue) : base(completionTime)
                => ReturnValue = returnValue;
        }

        public sealed class Failed : ScriptResult
        {
            public Exception Exception { get; }

            public Failed(TimeSpan completionTime, Exception exception) : base(completionTime) => Exception = exception;

            public LocalEmbed GetDiagnosticEmbed() => new LocalEmbed()
                .WithTitle(Exception.GetType().Name)
                .WithDescription(Markdown.CodeBlock(Exception.StackTrace))
                .WithColor(Color.Red)
                .WithFooter(@$"{CompletionTime:s\.ffff\s}");
        }
    }
}
