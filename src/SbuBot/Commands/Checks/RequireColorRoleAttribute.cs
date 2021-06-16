using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireColorRoleAttribute : DiscordGuildCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        public override async ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
            => (await context.GetOrCreateMemberAsync())!.ColorRole is { } == RequireColorRole
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                );
    }
}