using System;
using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.Checks.Parameters
{
    public class MustBeFutureAttribute : SbuParameterCheckAttribute
    {
        private const string FAILURE_REASON = "The given timestamp must be in the future.";

        protected override ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context)
        {
            DateTimeOffset now = DateTimeOffset.Now;

            return argument switch
            {
                TimeSpan timeSpan => now + timeSpan <= now
                    ? ParameterCheckAttribute.Failure(MustBeFutureAttribute.FAILURE_REASON)
                    : ParameterCheckAttribute.Success(),
                DateTime dateTime => dateTime <= now
                    ? ParameterCheckAttribute.Failure(MustBeFutureAttribute.FAILURE_REASON)
                    : ParameterCheckAttribute.Success(),
                DateTimeOffset dateTimeOffset => dateTimeOffset <= now
                    ? ParameterCheckAttribute.Failure(MustBeFutureAttribute.FAILURE_REASON)
                    : ParameterCheckAttribute.Success(),
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