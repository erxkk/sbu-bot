using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Group("edit")]
        [Description("A group of commands for editing color roles.")]
        public sealed class EditSubModule : SbuModuleBase
        {
            [Command]
            [Description("Modifies the authors color role's color and name.")]
            [Usage("role edit blue my blue role", "r change #afafaf yooo it's now gray")]
            public async Task<DiscordCommandResult> EditAsync(
                [Description("The new color.")] Color color,
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                string name
            )
            {
                SbuMember member = await Context.GetDbAuthorAsync();

                if (member.ColorRole is null)
                    return Reply("You must to have a color role to edit it.");

                if (Context.Guild.Roles.GetValueOrDefault(member.ColorRole!.Id) is not { } role)
                    return Reply(SbuUtility.Format.DoesNotExist("The role"));

                if (Context.CurrentMember.GetHierarchy() <= role.Position)
                    return Reply(SbuUtility.Format.HasHigherHierarchy("modify the role"));

                await role.ModifyAsync(
                    r =>
                    {
                        r.Color = color;
                        r.Name = name;
                    }
                );

                return Reply("Your role has been modified.");
            }

            [Command("name")]
            [Description("Modifies the authors color role's name.")]
            [Usage("role edit name new name")]
            public async Task<DiscordCommandResult> EditNameAsync(
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                string name
            )
            {
                SbuMember member = await Context.GetDbAuthorAsync();

                if (member.ColorRole is null)
                    return Reply("You must to have a color role to edit it.");

                if (Context.Guild.Roles.GetValueOrDefault(member.ColorRole!.Id) is not { } role)
                    return Reply(SbuUtility.Format.DoesNotExist("The role"));

                if (Context.CurrentMember.GetHierarchy() <= role.Position)
                    return Reply(SbuUtility.Format.HasHigherHierarchy("modify the role"));

                await role.ModifyAsync(r => r.Name = name);
                return Reply("Your role has been modified.");
            }

            [Command("color")]
            [Description("Modifies the authors color role's color.")]
            [Usage("role edit color blue", "sbu r change color #afafaf")]
            public async Task<DiscordCommandResult> EditColorAsync([Description("The new color.")] Color color)
            {
                SbuMember member = await Context.GetDbAuthorAsync();

                if (member.ColorRole is null)
                    return Reply("You must to have a color role to edit it.");

                if (Context.Guild.Roles.GetValueOrDefault(member.ColorRole!.Id) is not { } role)
                    return Reply(SbuUtility.Format.DoesNotExist("The role"));

                if (Context.CurrentMember.GetHierarchy() <= role.Position)
                    return Reply(SbuUtility.Format.HasHigherHierarchy("modify the role"));

                await role.ModifyAsync(r => r.Color = color);
                return Reply("Your role has been modified.");
            }
        }
    }
}