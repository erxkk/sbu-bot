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
        [Group("claim")]
        [RequireColorRole(false)]
        [Description("Claims the given color role if it has no owner.")]
        public sealed class ClaimGroup : SbuModuleBase
        {
            [Command]
            [Usage("role claim some role name", "r take 732234804384366602", "r claim @SBU-Bot")]
            public async Task<DiscordCommandResult> ClaimAsync(
                [MustBeOwned(false)][Description("The role to claim.")][Remarks("Must be a color role.")]
                SbuColorRole role
            )
            {
                // we ignore possible null values and just say it's higher lol
                if (Context.CurrentMember.GetHierarchy() <= Context.Guild.Roles.GetValueOrDefault(role.Id)?.Position)
                    await Reply("The role is higher than mine I can't assign it.");

                await Context.Author.GrantRoleAsync(role.Id);

                var context = Context.GetSbuDbContext();

                role.OwnerId = Context.Author.Id;
                context.ColorRoles.Update(role);
                await context.SaveChangesAsync();

                return Reply("Color role claimed.");
            }

            [Command]
            [Usage("role claim some role name", "r take 732234804384366602", "r claim @SBU-Bot")]
            public async Task<DiscordCommandResult> ClaimNewAsync(
                [MustBeColorRole, MustExistInDb(false)]
                [Description("The role to claim")]
                [Remarks("Must be a color role.")]
                IRole role
            )
            {
                if (Context.CurrentMember.GetHierarchy() <= role.Position)
                    await Reply("The role is higher than mine I can't assign it.");

                await Context.Author.GrantRoleAsync(role.Id);

                SbuDbContext context = Context.GetSbuDbContext();

                context.AddColorRole(role, Context.Author.Id);
                await context.SaveChangesAsync();

                return Reply("Color role claimed.");
            }
        }
    }
}