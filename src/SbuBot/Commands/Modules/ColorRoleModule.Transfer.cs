using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Command("transfer")]
        [RequireColorRole]
        [Description("Transfers the authors color role to the given member.")]
        [Usage("role transfer @user", "r transfer 352815253828141056", "r transfer Allah")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor, MustHaveColorRole(false)][Description("The member that should receive the color role.")]
            SbuMember receiver
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuColorRole role = (await context.GetMemberFullAsync(Context.Author))!.ColorRole!;

            await Context.Guild.GrantRoleAsync(receiver.Id, role.Id);
            await Context.Author.RevokeRoleAsync(role.Id);

            role.OwnerId = receiver.Id;
            context.ColorRoles.Update(role);
            await context.SaveChangesAsync();

            return Reply($"You transferred your color role to {Mention.User(receiver.Id)}.");
        }
    }
}