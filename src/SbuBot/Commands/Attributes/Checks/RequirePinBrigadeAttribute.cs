using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Gateway;

using Qmmands;

using SbuBot.Exceptions;

namespace SbuBot.Commands.Attributes.Checks
{
    public sealed class RequirePinBrigadeAttribute : DiscordGuildCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            if (context.Author.GetRoles() is not { Count: > 0 } roles)
                throw new NotCachedException("Could not find user roles in cache.");

            return !roles.ContainsKey(SbuGlobals.Role.Perm.PIN)
                ? Failure("You require the administrator role to use this command.")
                : Success();
        }
    }
}