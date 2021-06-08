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
    [Remarks("A user may only have one color role at a time.")]
    public sealed class ColorRoleModule : SbuModuleBase
    {
        [Group("claim", "take"), PureGroup, RequireAuthorColorRole(false)]
        [Description("Claims the given color role if it has no owner.")]
        public sealed class ClaimGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ClaimAsync([MustBeOwned(false)] SbuColorRole colorRole)
            {
                await Context.Author.GrantRoleAsync(colorRole.DiscordId);

                colorRole.OwnerId = Context.Author.Id;
                Context.Db.ColorRoles.Update(colorRole);
                await Context.Db.SaveChangesAsync();

                return Reply("Color role claimed.");
            }

            [Command]
            public async Task<DiscordCommandResult> ClaimNewAsync(
                [MustBeColorRole, MustExistInDb(false)]
                IRole colorRole
            )
            {
                await Context.Author.GrantRoleAsync(colorRole.Id);

                Context.Db.ColorRoles.Add(new(colorRole, Context.Author.Id));
                await Context.Db.SaveChangesAsync();

                return Reply("Color role claimed.");
            }
        }

        [Command("create", "make", "new"), RequireAuthorColorRole(false)]
        [Description("Creates a new color role.")]
        public async Task<DiscordCommandResult> CreateAsync(Color color, [Maximum(100)] string? name = null)
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

        [Group("edit", "change"), RequireAuthorColorRole]
        [Description("A group of commands for editing color roles.")]
        public class EditGroup : SbuModuleBase
        {
            [Command]
            [Description("Modifies the authors color role's color and name.")]
            public async Task<DiscordCommandResult> EditAsync(Color color, [Maximum(100)] string name)
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
            public async Task<DiscordCommandResult> SetNameAsync([Maximum(100)] string newName)
            {
                await Context.Guild.Roles[Context.Invoker.ColorRole!.DiscordId].ModifyAsync(r => r.Name = newName);
                return Reply("Your role has been modified.");
            }

            [Command("color")]
            [Description("Modifies the authors color role's color.")]
            public async Task<DiscordCommandResult> SetColorAsync(Color newColor)
            {
                await Context.Guild.Roles[Context.Invoker.ColorRole!.DiscordId].ModifyAsync(r => r.Color = newColor);
                return Reply("Your role has been modified.");
            }
        }

        [Command("remove", "delete"), RequireAuthorColorRole]
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

        [Command("transfer"), RequireAuthorColorRole]
        [Description("Transfers the authors color role to the given member.")]
        public async Task<DiscordCommandResult> TransferColorRoleAsync(
            [NotAuthor, MustHaveColorRole(false)] SbuMember receiver
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