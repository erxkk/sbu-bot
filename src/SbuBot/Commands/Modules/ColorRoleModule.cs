using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("role")]
    public sealed class ColorRoleModule : SbuModuleBase
    {
        // TODO: TEST
        [Group("claim", "take"), RequireAuthorColorRole(false)]
        public sealed class ClaimGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ClaimAsync([MustBeOwned(false)] SbuColorRole colorRole)
            {
                colorRole.OwnerId = Context.Author.Id;
                Context.Db.ColorRoles.Update(colorRole);
                await Context.Db.SaveChangesAsync();

                await Context.Author.GrantRoleAsync(colorRole.DiscordId);
                return Reply($"You now own {Mention.Role(colorRole.DiscordId)}.");
            }

            [Command]
            public async Task<DiscordCommandResult> ClaimNewAsync(
                [MustBeColorRole, MustExistInDb(false)]
                IRole colorRole
            )
            {
                Context.Db.ColorRoles.Add(new(colorRole, Context.Author.Id));
                await Context.Db.SaveChangesAsync();

                await Context.Author.GrantRoleAsync(colorRole.Id);
                return Reply($"You now own {colorRole.Mention}.");
            }
        }

        [Command("create"), RequireAuthorColorRole(false)]
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
                    return Reply("The role name msut be shorter than 100 characters.");

                name = waitNameResult?.Message?.Content ?? Context.Author.Nick ?? Context.Author.Name;
            }

            IRole role = await Context.Guild.CreateRoleAsync(
                r =>
                {
                    r.Color = color;
                    r.Name = name;
                }
            );

            Context.Db.ColorRoles.Add(new(role, Context.Author.Id));
            await Context.Db.SaveChangesAsync();

            await Context.Author.GrantRoleAsync(role.Id);
            return Reply($"{role.Mention} is your new color role.");
        }

        [Command("name"), RequireAuthorColorRole]
        public async Task<DiscordCommandResult> SetNameAsync([Maximum(100)] string newName)
        {
            await Context.Guild.Roles[Context.Invoker!.ColorRole!.DiscordId].ModifyAsync(r => r.Name = newName);
            return Reply("Your role has been modified.");
        }

        [Command("color"), RequireAuthorColorRole]
        public async Task<DiscordCommandResult> SetColorAsync(Color newColor)
        {
            await Context.Guild.Roles[Context.Invoker!.ColorRole!.DiscordId].ModifyAsync(r => r.Color = newColor);
            return Reply("Your role has been modified.");
        }

        // TODO: TEST
        [Command("remove", "delete"), RequireAuthorColorRole]
        public async Task<DiscordCommandResult> RemoveAsync()
        {
            Context.Db.ColorRoles.Remove(Context.Invoker!.ColorRole!);
            await Context.Db.SaveChangesAsync();

            await Context.Guild.Roles[Context.Invoker!.ColorRole!.DiscordId].DeleteAsync();
            return Reply("Your role has been removed.");
        }

        // TODO: TEST
        [Command("transfer"), RequireAuthorColorRole]
        public async Task<DiscordCommandResult> TransferColorRoleAsync(
            [NotAuthor] SbuMember member
        )
        {
            Context.Invoker!.ColorRole!.OwnerId = member.DiscordId;
            Context.Db.ColorRoles.Update(Context.Invoker.ColorRole);
            await Context.Db.SaveChangesAsync();

            await Context.Author.GrantRoleAsync(Context.Invoker!.ColorRole!.DiscordId);

            return Reply(
                string.Format(
                    "{0} now owns {1}.",
                    Mention.User(member.DiscordId),
                    Mention.Role(Context.Invoker.ColorRole.DiscordId)
                )
            );
        }
    }
}