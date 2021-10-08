using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks
{
    public sealed class RequireGuildConfigAttribute : DiscordGuildCheckAttribute
    {
        public SbuGuildConfig Config { get; }

        public RequireGuildConfigAttribute(SbuGuildConfig config) => Config = config;

        public override async ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
            => (await context.GetGuildAsync()).Config.HasFlag(Config)
                ? Success()
                : Failure(
                    $"The guild doesn't have this feature ({Config}) enabled."
                );
    }
}