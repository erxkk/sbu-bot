using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Command("delete")]
        [Description("Removes the authors color role.")]
        public async Task<DiscordCommandResult> DeleteAsync()
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (member!.ColorRole is null)
                return Reply("You must to have a color role to delete it.");

            if (Context.Guild.Roles.GetValueOrDefault(member.ColorRole!.Id) is not { } role)
                return Response(SbuUtility.Format.DoesNotExist("The role"));

            if (Context.CurrentMember.GetHierarchy() <= role.Position)
                return Response(SbuUtility.Format.HasHigherHierarchy("delete the role"));

            await ConsistencyService.IgnoreRoleRemovedAsync(role.Id);
            await role.DeleteAsync();

            context.ColorRoles.Remove(member.ColorRole);
            await context.SaveChangesAsync();

            return Response("Your color role has been removed.");
        }
    }
}
