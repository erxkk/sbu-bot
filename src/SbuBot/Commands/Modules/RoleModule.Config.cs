using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class RoleModule : SbuModuleBase
    {
        [Command("create")]
        [RequireAuthorGuildPermissions(Permission.ManageRoles)]
        [Description("Add a role to the requestable roles.")]
        public async Task<DiscordCommandResult> CreateAsync(
            [RequireHierarchy(HierarchyComparison.Less, HierarchyComparisonContext.Bot)]
            [Description("The role to add to the requestable roles.")]
            IRole role,
            [Description("An optional description for this role.")]
            string? description = null
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();

            if (await context.GetRoleAsync(role) is { })
                return Reply("This role was already added to the requestable roles.");

            context.AddRole(role, description);
            await context.SaveChangesAsync();

            return Reply($"{Mention.Role(role.Id)} was added to the requestable roles.");
        }

        [Command("delete")]
        [RequireAuthorGuildPermissions(Permission.ManageRoles)]
        [Description("Remove a role from the requestable roles.")]
        public async Task<DiscordCommandResult> RemoveAsync(
            [RequireHierarchy(HierarchyComparison.Less, HierarchyComparisonContext.Bot)]
            [Description("The role to remove from the requestable roles.")]
            IRole role
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();

            if (await context.GetRoleAsync(role) is not { } sbuRole)
                return Reply("This role wasn't previously added to the requestable roles.");

            context.Roles.Remove(sbuRole);
            await context.SaveChangesAsync();

            return Reply($"{Mention.Role(role.Id)} was removed form the requestable roles.");
        }
    }
}