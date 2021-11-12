using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Views;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("colorrole", "crole", "cr")]
    [RequireBotGuildPermissions(Permission.ManageRoles)]
    [Description("A collection of commands for creation, modification, removal and usage of color roles.")]
    [Remarks(
        "A user may only have one color role at a time, role colors can be given as hex codes starting with `#` or as "
        + "color name literals."
    )]
    public sealed partial class ColorRoleModule : SbuModuleBase
    {
        public ConsistencyService ConsistencyService { get; set; } = null!;

        [Command]
        [Description("Displays information about your current color role.")]
        public DiscordCommandResult Role() => Context.Author.GetColorRole() is { } role
            ? Reply(
                new LocalEmbed()
                    .WithColor(role.Color)
                    .AddField("Name", role.Name)
                    .AddField("Color", role.Color)
            )
            : Reply("You have no color role.");

        [Command("create")]
        [Description("Creates a new color role.")]
        public async Task<DiscordCommandResult> CreateAsync(
            [Description("The role color.")] Color color,
            [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
            [Description("The role name.")]
            [Remarks("Cannot be longer than 100 characters.")]
            [UsageOverride("my role name", "funny role")]
            string? name = null
        )
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (member!.ColorRole is { })
                return Reply("You must to have no color role to create one.");

            if (name is null)
            {
                switch (await Context.WaitFollowUpForAsync("What do you want the role name to be?"))
                {
                    case Result<string, FollowUpError>.Success followUp:
                    {
                        name = followUp.Value;

                        if (name.Length > SbuColorRole.MAX_NAME_LENGTH)
                            return Reply("The role name must be shorter than 100 characters.");

                        break;
                    }

                    case Result<string, FollowUpError>.Error error:
                    {
                        if (error.Value == FollowUpError.Aborted)
                            return Reply("Aborted");

                        await Reply("You didn't provide a role name so i just named it after yourself.");

                        name = Context.Author.Nick ?? Context.Author.Name;
                        break;
                    }
                }
            }

            SbuGuild? guild = await context.GetGuildAsync(Context.Guild);
            int? rolePos = Context.Guild.Roles.GetValueOrDefault(guild!.ColorRoleBottomId ?? 0)?.Position + 1;

            IRole role = await Context.Guild.CreateRoleAsync(
                r =>
                {
                    r.Color = color;
                    r.Name = name;
                }
            );

            if (rolePos is { })
            {
                if (Context.CurrentMember.GetHierarchy() <= rolePos)
                {
                    // the role is not yet added to the db so we can ignore the deletion to safe us a db call
                    await ConsistencyService.IgnoreRoleRemovedAsync(role.Id);
                    await role.DeleteAsync();
                    return Reply(SbuUtility.Format.HasHigherHierarchy("move the role"));
                }

                await role.ModifyAsync(r => r.Position = rolePos.Value);
            }

            await Context.Author.GrantRoleAsync(role.Id);
            context.AddColorRole(role, Context.Author.Id);
            await context.SaveChangesAsync();

            return Reply($"{role.Mention} is your new color role.");
        }

        [Command("delete")]
        [Description("Removes the authors color role.")]
        public async Task<DiscordCommandResult> DeleteAsync()
        {
            SbuDbContext context = Context.GetSbuDbContext();
            SbuMember? member = await context.GetMemberFullAsync(Context.Author);

            if (member!.ColorRole is null)
                return Reply("You must to have a color role to delete it.");

            if (Context.Guild.Roles.GetValueOrDefault(member.ColorRole!.Id) is not { } role)
            {
                await Reply(SbuUtility.Format.DoesNotExist("The role"));
            }
            else if (Context.CurrentMember.GetHierarchy() <= role.Position)
            {
                await Reply(SbuUtility.Format.HasHigherHierarchy("delete the role"));
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

            return Reply("Color role claimed.");
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

            return Reply($"You transferred your color role to {Mention.User(receiver.Id)}.");
        }
    }
}
