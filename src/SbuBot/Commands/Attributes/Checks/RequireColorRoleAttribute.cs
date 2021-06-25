using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Attributes.Checks
{
    public sealed class RequireColorRoleAttribute : DiscordGuildCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        public override async ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
            => (await context.GetSbuDbContext().GetMemberAsync(context.Author))!.ColorRole is { } == RequireColorRole
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                );
    }
}