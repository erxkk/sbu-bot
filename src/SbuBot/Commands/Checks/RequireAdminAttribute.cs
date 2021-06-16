using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Gateway;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireAdminAttribute : DiscordGuildCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            if (context.Author.GetRoles() is not { Count: > 0 } roles)
                throw new RequiredCacheException("Could not find user roles in cache.");

            return !roles.ContainsKey(SbuGlobals.Role.ADMIN)
                ? CheckAttribute.Failure("You require the administrator role to use this command.")
                : CheckAttribute.Success();
        }
    }
}