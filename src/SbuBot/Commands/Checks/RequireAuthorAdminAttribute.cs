using System.Threading.Tasks;

using Disqord.Gateway;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireAuthorAdminAttribute : SbuCheckAttribute
    {
        protected override ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
        {
            if (context.Author.GetRoles() is not { Count: > 0 } roles)
                throw new RequiredCacheException("Could not find user roles in cache.");

            return roles.ContainsKey(SbuBotGlobals.Guild.Roles.Permissions.ADMIN)
                ? CheckAttribute.Failure("You require the administrator role to use this command.")
                : CheckAttribute.Success();
        }
    }
}