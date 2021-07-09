using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Attributes.Checks
{
    public sealed class RequireColorRoleAttribute : DiscordGuildCheckAttribute
    {
        public bool RequireColorRole { get; }

        public RequireColorRoleAttribute(bool requireColorRole = true) => RequireColorRole = requireColorRole;

        public override async ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
            => (await context.GetSbuDbContext()
                    .GetMemberAsync(context.Author, m => m.Include(m => m.ColorRole))).ColorRole
                is { }
                == RequireColorRole
                    ? Success()
                    : Failure(
                        $"You must to have {(RequireColorRole ? "a" : "no")} color role to use this command."
                    );
    }
}