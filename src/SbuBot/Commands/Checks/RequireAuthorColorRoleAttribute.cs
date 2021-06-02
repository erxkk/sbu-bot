using System.Linq;
using System.Threading.Tasks;

using Disqord.Gateway;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireAuthorColorRoleAttribute : SbuCheckAttribute
    {
        protected override ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
        {
            if (context.Author.GetRoles() is not { Count: > 1 } roles)
                throw new RequiredCacheException("Could not find color role separator in cache.");

            if (!context.Guild.Roles.TryGetValue(
                SbuBotGlobals.Roles.Categories.COLOR,
                out var colorRoleSeparator
            )) throw new RequiredCacheException("Could not find color role separator in cache.");

            return roles.Values
                .Where(r => r.Position < colorRoleSeparator.Position)
                .OrderByDescending(r => r.Position)
                .FirstOrDefault(r => r.Color is { }) is null
                ? CheckAttribute.Failure("You need ot have a color role to use this command.")
                : CheckAttribute.Success();
        }
    }
}