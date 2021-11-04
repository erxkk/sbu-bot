using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes.Checks;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Command("delete")]
        [RequireColorRole]
        [Description("Removes the authors color role.")]
        public async Task<DiscordCommandResult> DeleteAsync()
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (Context.Guild.Roles.GetValueOrDefault(member!.ColorRole!.Id) is not { } role)
            {
                await Reply(ColorRoleModule.ROLE_DOES_NOT_EXIST);
            }
            else if (Context.CurrentMember.GetHierarchy() <= role.Position)
            {
                await Reply(string.Format(ColorRoleModule.ROLE_HAS_HIGHER_HIERARCHY_FORMAT, "delete"));
            }
            else
            {
                await ConsistencyService.IgnoreRoleRemovedAsync(role.Id);
                await role.DeleteAsync();
            }

            context.ColorRoles.Remove(member.ColorRole);
            await context.SaveChangesAsync();

            return Reply("Your color role has been removed.");
        }
    }
}