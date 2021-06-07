using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireAuthorColorRoleAttribute : SbuCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireAuthorColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        protected override ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
        {
            if (!context.Guild.Roles.ContainsKey(context.Invoker!.ColorRole!.DiscordId))
            {
                throw new RequiredCacheException(
                    $"Could not find required color role ({context.Invoker!.ColorRole!.DiscordId}) in cache."
                );
            }

            return context.Invoker!.ColorRole is { } == RequireColorRole
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                );
        }
    }
}