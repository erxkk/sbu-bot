using System.Threading.Tasks;

using Disqord.Gateway;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequirePinBrigadeAttribute : SbuCheckAttribute
    {
        protected override ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
        {
            if (context.Author.GetRoles() is not { Count: > 0 } roles)
                throw new RequiredCacheException("Could not find user roles in cache.");

            return !roles.ContainsKey(SbuGlobals.Role.Perm.PIN)
                ? CheckAttribute.Failure("You require the administrator role to use this command.")
                : CheckAttribute.Success();
        }
    }
}