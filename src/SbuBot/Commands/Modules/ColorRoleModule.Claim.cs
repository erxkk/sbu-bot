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
        [Group("claim")]
        [RequireColorRole(false)]
        [Description("Claims the given color role if it has no owner.")]
        public sealed class ClaimGroup : SbuModuleBase
        {
            [Command]
            [Usage("role claim some role name", "r take 732234804384366602", "r claim @SBU-Bot")]
            public async Task<DiscordCommandResult> ClaimAsync(
                [RequireHigherHierarchy, MustBeOwned(false)]
                [Description("The role to claim.")]
                [Remarks("Must be a color role.")]
                SbuColorRole role
            )
            {
                await Context.Author.GrantRoleAsync(role.Id);

                role.OwnerId = Context.Author.Id;
                Context.GetSbuDbContext().ColorRoles.Update(role);
                await Context.SaveChangesAsync();

                return Reply("Color role claimed.");
            }

            [Command]
            [Usage("role claim some role name", "r take 732234804384366602", "r claim @SBU-Bot")]
            public async Task<DiscordCommandResult> ClaimNewAsync(
                [MustBeColorRole, RequireHigherHierarchy, MustExistInDb(false)]
                [Description("The role to claim")]
                [Remarks("Must be a color role.")]
                IRole role
            )
            {
                await Context.Author.GrantRoleAsync(role.Id);
                Context.GetSbuDbContext().AddColorRole(role, Context.Author.Id);
                await Context.SaveChangesAsync();

                return Reply("Color role claimed.");
            }
        }
    }
}