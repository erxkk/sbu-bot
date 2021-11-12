using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public class MustBeFutureAttribute : DiscordGuildParameterCheckAttribute
    {
        private const string FAILURE_REASON = "The given timestamp must be in the future.";

        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            DateTimeOffset now = DateTimeOffset.Now;

            return argument switch
            {
                TimeSpan timeSpan => now + timeSpan <= now
                    ? Failure(MustBeFutureAttribute.FAILURE_REASON)
                    : Success(),
                DateTime dateTime => dateTime <= now
                    ? Failure(MustBeFutureAttribute.FAILURE_REASON)
                    : Success(),
                DateTimeOffset dateTimeOffset => dateTimeOffset <= now
                    ? Failure(MustBeFutureAttribute.FAILURE_REASON)
                    : Success(),
                _ => throw new ArgumentOutOfRangeException(),
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