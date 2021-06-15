using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireColorRoleAttribute : SbuCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        protected override async ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
            => (await context.GetOrCreateMemberAsync())!.ColorRole is { } == RequireColorRole
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                );
    }
}