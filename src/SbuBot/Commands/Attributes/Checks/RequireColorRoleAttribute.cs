using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks
{
    public sealed class RequireColorRoleAttribute : DiscordGuildCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        public override async ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            SbuMember member = await context
                .GetSbuDbContext()
                .GetMemberAsync(context.Author, m => m.Include(m => m.ColorRole));

            return member.ColorRole is { } == RequireColorRole
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                );
        }
    }
}