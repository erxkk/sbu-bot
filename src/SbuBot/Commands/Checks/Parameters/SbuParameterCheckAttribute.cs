using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Checks.Parameters
{
    public abstract class SbuParameterCheckAttribute : DiscordGuildParameterCheckAttribute
    {
        protected abstract ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context);

        public sealed override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            if (context is not SbuCommandContext ctx)
                throw new InvalidOperationException($"Invalid Context type: {context.GetType().Name}");

            return CheckAsync(argument, ctx);
        }
    }
}