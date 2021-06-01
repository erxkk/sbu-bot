using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public abstract class SbuCheckAttribute : DiscordGuildCheckAttribute
    {
        protected abstract ValueTask<CheckResult> CheckAsync(SbuCommandContext context);

        public sealed override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            if (context is not SbuCommandContext ctx)
                throw new InvalidOperationException($"Invalid Context type: {context.GetType().Name}");

            return CheckAsync(ctx);
        }
    }
}