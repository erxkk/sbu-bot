using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("role", "r")]
    [RequireBotGuildPermissions(Permission.ManageRoles)]
    [Description("A group of commands for managing and requesting roles.")]
    public sealed partial class RoleModule : SbuModuleBase
    {
        [Command("list")]
        public async Task<DiscordCommandResult> ListAsync()
        {
            SbuGuild? guild = await Context.GetSbuDbContext()
                .GetGuildAsync(Context.Guild, q => q.Include(g => g.Roles));

            return Reply(
                new LocalEmbed()
                    .WithTitle("Available Roles")
                    .WithDescription(
                        guild!.Roles.Select(
                                r => $"{SbuGlobals.BULLET} {Mention.Role(r.Id)}\n{r.Description ?? "`No Description`"}"
                            )
                            .ToNewLines()
                    )
            );
        }

        [Command("get")]
        [Description("Add a role to yourself.")]
        public async Task<DiscordCommandResult> GetAsync(
            [RequireHierarchy(HierarchyComparison.Less, HierarchyComparisonContext.Bot)]
            [Description("The role to add.")]
            IRole role
        )
        {
            SbuGuild? guild = await Context.GetSbuDbContext()
                .GetGuildAsync(Context.Guild, q => q.Include(g => g.Roles));

            if (guild!.Roles.Select(r => r.Id).Contains(role.Id))
                return Reply("This role cannot be requested.");

            if (Context.Author.RoleIds.Contains(role.Id))
                return Reply("You already have this role.");

            await Context.Author.GrantRoleAsync(role.Id);

            return Reply($"You now have {role.Mention}.");
        }

        [Command("leave")]
        [Description("Remove a role from yourself.")]
        public async Task<DiscordCommandResult> LeaveAsync(
            [RequireHierarchy(HierarchyComparison.Less, HierarchyComparisonContext.Bot)]
            [Description("The role to remove.")]
            IRole role
        )
        {
            SbuGuild? guild = await Context.GetSbuDbContext()
                .GetGuildAsync(Context.Guild, q => q.Include(g => g.Roles));

            if (guild!.Roles.Select(r => r.Id).Contains(role.Id))
                return Reply("This role cannot be removed.");

            if (!Context.Author.RoleIds.Contains(role.Id))
                return Reply("You don't have this role.");

            await Context.Author.RevokeRoleAsync(role.Id);

            return Reply($"You no longer have {role.Mention}.");
        }
    }
}
