using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
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
            SbuColorRole sbuRole = (await context.GetMemberFullAsync(Context.Author))!.ColorRole!;

            if (Context.Guild.Roles.GetValueOrDefault(sbuRole.Id) is not { } role)
                return Reply(ColorRoleModule.ROLE_DOES_NOT_EXIST);

            if (Context.CurrentMember.GetHierarchy() <= role.Position)
                return Reply(string.Format(ColorRoleModule.ROLE_HAS_HIGHER_HIERARCHY_FORMAT, "modify"));

            await Context.Guild.GrantRoleAsync(receiver.Id, sbuRole.Id);
            await Context.Author.RevokeRoleAsync(sbuRole.Id);

            sbuRole.OwnerId = receiver.Id;
            context.ColorRoles.Update(sbuRole);
            await context.SaveChangesAsync();

            return Reply($"You transferred your color role to {Mention.User(receiver.Id)}.");
        }
    }
}