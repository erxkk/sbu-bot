using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes.Checks;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Command("delete")]
        [RequireColorRole]
        [Description("Removes the authors color role.")]
        public async Task<DiscordCommandResult> DeleteAsync()
        {
            ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
            SbuMember member = await Context.GetAuthorAsync();
            service.IgnoreRemovedRole(member.ColorRole!.Id);

            await Context.Guild.Roles[member.ColorRole.Id].DeleteAsync();

            Context.GetSbuDbContext().ColorRoles.Remove(member.ColorRole);
            await Context.SaveChangesAsync();

            return Reply("Your color role has been removed.");
        }
    }
}