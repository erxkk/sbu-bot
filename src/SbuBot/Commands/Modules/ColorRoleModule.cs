using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Commands.Information;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("role")]
    [Description("A collection of commands for creation, modification, removal and usage of color roles.")]
    [Remarks(
        "A user may only have one color role at a time, role colors can be given as hex codes starting with `#` or as "
        + "color name literals."
    )]
    public sealed class ColorRoleModule : SbuModuleBase
    {
        [Group("claim", "take"), PureGroup, RequireColorRole(false)]
        [Description("Claims the given color role if it has no owner.")]
        public sealed class ClaimGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ClaimAsync(
                [MustBeOwned(false)][Description("The role to claim")][Remarks("Must be a color role.")]
                SbuColorRole role
            )
            {
                await Context.Author.GrantRoleAsync(role.DiscordId);

                role.OwnerId = Context.Author.Id;
                Context.Db.ColorRoles.Update(role);
                await Context.Db.SaveChangesAsync();

                return Reply("Color role claimed.");
            }

            [Command]
            public async Task<DiscordCommandResult> ClaimNewAsync(
                [MustBeColorRole, MustExistInDb(false)]
                [Description("The role to claim")]
                [Remarks("Must be a color role.")]
                IRole role
            )
            {
                await Context.Author.GrantRoleAsync(role.Id);

                Context.Db.ColorRoles.Add(new(role, Context.Author.Id));
                await Context.Db.SaveChangesAsync();

                return Reply("Color role claimed.");
            }
        }

        [Command("create", "make", "new"), RequireColorRole(false)]
        [Description("Creates a new color role.")]
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
                MessageReceivedEventArgs? waitNameResult;
                await Reply("What do you want the role name to be?");

                await using (Context.BeginYield())
                {
                    waitNameResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitNameResult is null)
                    await Reply("You didn't provide a role name so i just named it after yourself.");
                else if (waitNameResult.Message.Content.Length > 100)
                    return Reply("The role name must be shorter than 100 characters.");

                name = waitNameResult?.Message?.Content ?? Context.Author.Nick ?? Context.Author.Name;
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

            Context.Db.ColorRoles.Add(new(role, Context.Author.Id));
            await Context.Db.SaveChangesAsync();

            return Reply($"{role.Mention} is your new color role.");
        }

        [Group("edit", "change"), RequireColorRole]
        [Description("A group of commands for editing color roles.")]
        public sealed class EditGroup : SbuModuleBase
        {
            [Command]
            [Description("Modifies the authors color role's color and name.")]
            public async Task<DiscordCommandResult> EditAsync(
                [Description("The new color.")] Color color,
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                string name
            )
            {
                await Context.Guild.Roles[Context.Invoker.ColorRole!.DiscordId]
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
            public async Task<DiscordCommandResult> SetNameAsync(
                [Maximum(SbuColorRole.MAX_NAME_LENGTH)]
                [Description("The new name.")]
                [Remarks("Cannot be longer than 100 characters.")]
                string name
            )
            {
                await Context.Guild.Roles[Context.Invoker.ColorRole!.DiscordId].ModifyAsync(r => r.Name = name);
                return Reply("Your role has been modified.");
            }

            [Command("color")]
            [Description("Modifies the authors color role's color.")]
            public async Task<DiscordCommandResult> SetColorAsync([Description("The new color.")] Color color)
            {
                await Context.Guild.Roles[Context.Invoker.ColorRole!.DiscordId].ModifyAsync(r => r.Color = color);
                return Reply("Your role has been modified.");
            }
        }

        [Command("remove", "delete"), RequireColorRole]
        [Description("Removes the authors color role.")]
        public async Task<DiscordCommandResult> RemoveAsync()
        {
            ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
            service.IgnoreRemovedRole(Context.Invoker.ColorRole!.DiscordId);

            await Context.Guild.Roles[Context.Invoker.ColorRole.DiscordId].DeleteAsync();

            Context.Db.ColorRoles.Remove(Context.Invoker.ColorRole);
            await Context.Db.SaveChangesAsync();

            return Reply("Your color role has been removed.");
        }

        [Command("transfer"), RequireColorRole]
        [Description("Transfers the authors color role to the given member.")]
        public async Task<DiscordCommandResult> TransferColorRoleAsync(
            [NotAuthor, MustHaveColorRole(false)][Description("The member that should receive the color role.")]
            SbuMember receiver
        )
        {
            SbuColorRole role = Context.Invoker.ColorRole!;

            ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
            service.IgnoreAddedRole(role.DiscordId);

            await Context.Guild.GrantRoleAsync(receiver.DiscordId, role.DiscordId);
            await Context.Author.RevokeRoleAsync(role.DiscordId);

            role.OwnerId = receiver.DiscordId;
            Context.Db.ColorRoles.Update(role);
            await Context.Db.SaveChangesAsync();

            return Reply($"You transferred your color role to {Mention.User(receiver.DiscordId)}.");
        }
    }
}