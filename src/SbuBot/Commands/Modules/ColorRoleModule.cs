using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("role", "r")]
    [RequireBotGuildPermissions(Permission.ManageRoles)]
    [Description("A collection of commands for creation, modification, removal and usage of color roles.")]
    [Remarks(
        "A user may only have one color role at a time, role colors can be given as hex codes starting with `#` or as "
        + "color name literals."
    )]
    public sealed class ColorRoleModule : SbuModuleBase
    {
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

        [Group("claim", "take")]
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
                await Context.GetSbuDbContext().SaveChangesAsync();

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
                await Context.GetSbuDbContext().SaveChangesAsync();

                return Reply("Color role claimed.");
            }
        }

        [Command("create", "make", "mk")]
        [RequireColorRole(false)]
        [Description("Creates a new color role.")]
        [Usage("role create #afafaf my gray role", "r make green dream role dream role")]
        public async Task<DiscordCommandResult> CreateAsync(
            [Description("The role color.")] Color color,
            [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
            [Description("The role name.")]
            [Remarks("Cannot be longer than 100 characters.")]
            string? name = null
        )
        {
            if (name is null)
            {
                await Reply("What do you want the role name to be?");

                if (await Context.WaitFollowUpForAsync() is Result<string?, Unit>.Success followUp)
                    name = followUp.Value;

                if (name is null)
                    await Reply("You didn't provide a role name so i just named it after yourself.");
                else if (name.Length > 100)
                    return Reply("The role name must be shorter than 100 characters.");

                name ??= Context.Author.Nick ?? Context.Author.Name;
            }

            IRole role = await Context.Guild.CreateRoleAsync(
                r =>
                {
                    r.Color = color;
                    r.Name = name;
                }
            );

            ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
            service.IgnoreAddedRole(role.Id);

            await Context.Author.GrantRoleAsync(role.Id);
            Context.GetSbuDbContext().AddColorRole(role, Context.Author.Id);
            await Context.GetSbuDbContext().SaveChangesAsync();

            return Reply($"{role.Mention} is your new color role.");
        }

        [Group("edit", "change")]
        [RequireColorRole]
        [Description("A group of commands for editing color roles.")]
        public sealed class EditGroup : SbuModuleBase
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
                SbuMember member = (await Context.GetSbuDbContext().GetMemberAsync(Context.Author))!;

                await Context.Guild.Roles[member.ColorRole!.Id]
                    .ModifyAsync(
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
            public async Task<DiscordCommandResult> SetNameAsync(
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                string name
            )
            {
                SbuMember member = await Context.GetAuthorAsync();
                await Context.Guild.Roles[member.ColorRole!.Id].ModifyAsync(r => r.Name = name);
                return Reply("Your role has been modified.");
            }

            [Command("color")]
            [Description("Modifies the authors color role's color.")]
            [Usage("role edit color blue", "sbu r change color #afafaf")]
            public async Task<DiscordCommandResult> SetColorAsync([Description("The new color.")] Color color)
            {
                SbuMember member = await Context.GetAuthorAsync();
                await Context.Guild.Roles[member.ColorRole!.Id].ModifyAsync(r => r.Color = color);
                return Reply("Your role has been modified.");
            }
        }

        [Command("remove", "rm", "delete")]
        [RequireColorRole]
        [Description("Removes the authors color role.")]
        public async Task<DiscordCommandResult> RemoveAsync()
        {
            ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
            SbuMember member = await Context.GetAuthorAsync();
            service.IgnoreRemovedRole(member.ColorRole!.Id);

            await Context.Guild.Roles[member.ColorRole.Id].DeleteAsync();

            Context.GetSbuDbContext().ColorRoles.Remove(member.ColorRole);
            await Context.GetSbuDbContext().SaveChangesAsync();

            return Reply("Your color role has been removed.");
        }

        [Command("transfer", "mv")]
        [RequireColorRole]
        [Description("Transfers the authors color role to the given member.")]
        [Usage("role transfer @user", "r transfer 352815253828141056", "r transfer Allah")]
        public async Task<DiscordCommandResult> TransferColorRoleAsync(
            [NotAuthor, MustHaveColorRole(false)][Description("The member that should receive the color role.")]
            SbuMember receiver
        )
        {
            SbuColorRole role = (await Context.GetAuthorAsync()).ColorRole!;

            ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
            service.IgnoreAddedRole(role.Id);

            await Context.Guild.GrantRoleAsync(receiver.Id, role.Id);
            await Context.Author.RevokeRoleAsync(role.Id);

            role.OwnerId = receiver.Id;
            Context.GetSbuDbContext().ColorRoles.Update(role);
            await Context.GetSbuDbContext().SaveChangesAsync();

            return Reply($"You transferred your color role to {Mention.User(receiver.Id)}.");
        }
    }
}