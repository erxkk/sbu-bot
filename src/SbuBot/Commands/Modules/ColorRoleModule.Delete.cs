using System.Threading.Tasks;

using Disqord.Bot;
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
            var context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            await Context.Guild.Roles[member!.ColorRole!.Id].DeleteAsync();

            context.ColorRoles.Remove(member.ColorRole);
            await context.SaveChangesAsync();

            return Reply("Your color role has been removed.");
        }
    }
}