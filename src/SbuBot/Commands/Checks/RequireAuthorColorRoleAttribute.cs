using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireAuthorColorRoleAttribute : SbuCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireAuthorColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        protected override ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
            => context.Invoker!.ColorRole is { } == RequireColorRole
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                );
    }
}