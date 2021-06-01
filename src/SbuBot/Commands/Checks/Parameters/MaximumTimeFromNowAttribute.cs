using System;
using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.Checks.Parameters
{
    public class MaximumTimeFromNowAttribute : SbuParameterCheckAttribute
    {
        public int Days { get; init; }
        public int Hours { get; init; }
        public int Minutes { get; init; }
        public int Seconds { get; init; }
        public int Milliseconds { get; init; }

        private TimeSpan TimeSpan => new(Days, Hours, Minutes, Seconds, Milliseconds);
        private string FailureReason => $"The given timestamp must not be further than `{TimeSpan}` in the future.";

        protected override ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context)
        {
            return argument switch
            {
                TimeSpan timeSpan => DateTimeOffset.Now + TimeSpan >= DateTimeOffset.Now + timeSpan
                    ? ParameterCheckAttribute.Success()
                    : ParameterCheckAttribute.Failure(FailureReason),
                DateTime dateTime => DateTimeOffset.Now + TimeSpan >= dateTime
                    ? ParameterCheckAttribute.Success()
                    : ParameterCheckAttribute.Failure(FailureReason),
                DateTimeOffset dateTimeOffset => DateTimeOffset.Now + TimeSpan >= dateTimeOffset
                    ? ParameterCheckAttribute.Success()
                    : ParameterCheckAttribute.Failure(FailureReason),
                null => ParameterCheckAttribute.Success(),
                _ => throw new ArgumentException(null, nameof(argument)),
            };
        }

        public override bool CheckType(Type type)
            => type == typeof(TimeSpan)
                || type == typeof(TimeSpan?)
                || type == typeof(DateTime)
                || type == typeof(DateTime?)
                || type == typeof(DateTimeOffset)
                || type == typeof(DateTimeOffset?);
    }
}