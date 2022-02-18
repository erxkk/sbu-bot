using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Views;
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
            public async Task<DiscordCommandResult> EditAsync(
                [Description("The new color.")] Color color,
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                [UsageOverride("new role name", "unfunny role")]
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

                return Response("Your role has been modified.");
            }

            [Command("name")]
            [Description("Modifies the authors color role's name.")]
            public async Task<DiscordCommandResult> EditNameAsync(
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                [UsageOverride("my role name", "funny role")]
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
                return Response("Your role has been modified.");
            }

            [Command("color")]
            [Description("Modifies the authors color role's color.")]
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
                return Response("Your role has been modified.");
            }
        }

        [Command("claim")]
        [Description("Claims the given color role if it has no owner.")]
        public async Task<DiscordCommandResult> ClaimAsync(
            [MustBeOwned(false), RequireHierarchy(HierarchyComparison.Less, HierarchyComparisonContext.Bot)]
            [Description("The role to claim.")]
            [Remarks("Must be a color role.")]
            SbuColorRole role
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (member!.ColorRole is { })
                return Reply("You must to have no color role to claim one.");

            await Context.Author.GrantRoleAsync(role.Id);

            role.OwnerId = Context.Author.Id;
            context.ColorRoles.Update(role);
            await context.SaveChangesAsync();

            return Response("Color role claimed.");
        }

        [Command("transfer")]
        [Description("Transfers the authors color role to the given member.")]
        public async Task<DiscordCommandResult> TransferAsync(
            [NotAuthor][Description("The member that should receive the color role.")]
            SbuMember receiver
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (member!.ColorRole is null)
                return Reply("You must to have a color role to transfer it.");

            if (receiver.ColorRole is { })
                return Reply("The receiver must to have no color role for you to transfer it to them.");

            if (Context.Guild.Roles.GetValueOrDefault(member.ColorRole!.Id) is not { } role)
                return Reply(SbuUtility.Format.DoesNotExist("The role"));

            if (Context.CurrentMember.GetHierarchy() <= role.Position)
                return Reply(SbuUtility.Format.HasHigherHierarchy("transfer the role"));

            ConfirmationState result = await ConfirmationAsync(receiver.Id, "Do you accept the role transfer?");

            switch (result)
            {
                case ConfirmationState.None:
                case ConfirmationState.Aborted:
                    return null!;

                case ConfirmationState.TimedOut:
                    return Reply("Aborted.");

                case ConfirmationState.Confirmed:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            await Context.Guild.GrantRoleAsync(receiver.Id, member.ColorRole.Id);
            await Context.Author.RevokeRoleAsync(member.ColorRole.Id);

            member.ColorRole.OwnerId = receiver.Id;
            context.ColorRoles.Update(member.ColorRole);
            await context.SaveChangesAsync();

            return Response($"You transferred your color role to {Mention.User(receiver.Id)}.");
        }
    }
}
