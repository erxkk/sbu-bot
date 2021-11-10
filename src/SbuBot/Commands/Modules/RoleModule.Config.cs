using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Attributes.Checks.Parameters;

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
            IRole role
        )
        {
            // TODO
            return Reply($"You now have {role}.");
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
            // TODO
            return Reply($"You no longer have {role}.");
        }
    }
}