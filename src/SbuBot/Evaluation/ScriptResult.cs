using System;

using Disqord;

namespace SbuBot.Evaluation
{
    public abstract class ScriptResult
    {
        public TimeSpan CompletionTime { get; }

        private ScriptResult(TimeSpan completionTime) => CompletionTime = completionTime;

        public virtual LocalEmbed ToEmbed() => new LocalEmbed().WithFooter(@$"{CompletionTime:s\.ffff\s}");

        public sealed class Completed : ScriptResult
        {
            public object? ReturnValue { get; }

            public Completed(TimeSpan completionTime, object? returnValue) : base(completionTime)
                => ReturnValue = returnValue;

            // TODO: write inspection methods for verbose info on return values
            public override LocalEmbed ToEmbed() => base.ToEmbed()
                .WithTitle("Result")
                .WithDescription(Markdown.CodeBlock(ReturnValue?.ToString() ?? "{null}"))
                .WithColor(Color.Green);
        }

        public sealed class Failed : ScriptResult
        {
            public Exception Exception { get; }

            public Failed(TimeSpan completionTime, Exception exception) : base(completionTime) => Exception = exception;

            public override LocalEmbed ToEmbed() => base.ToEmbed()
                .WithTitle(Exception.GetType().Name)
                .WithDescription(Markdown.CodeBlock(Exception.StackTrace))
                .WithColor(Color.Red);
        }
    }
}