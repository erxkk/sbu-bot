using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed class OwnershipModule : SbuModuleBase
    {
        [Group("inherit")]
        public sealed class InheritGroup : SbuModuleBase
        {
            [Command, RequireAuthorAdmin]
            public Task<DiscordCommandResult> InheritMemberAsync(
                [NotAuthor] IMember deceased,
                [NotAuthor] IMember inheritor
            ) => _inheritMemberAsync(deceased.Id, inheritor);

            [Command, RequireAuthorInDb]
            public Task<DiscordCommandResult> AuthorInheritMemberAsync([NotAuthor] Snowflake deceasedId)
                => !Context.Guild.Members.ContainsKey(deceasedId)
                    ? Task.FromResult(Reply("The given user must be deceased.") as DiscordCommandResult)
                    : _inheritMemberAsync(deceasedId, Context.Author);

            private async Task<DiscordCommandResult> _inheritMemberAsync(
                Snowflake deceasedId,
                IMember inheritor
            )
            {
                bool notAuthor = inheritor.Id != Context.Author.Id;

                if (deceasedId == inheritor.Id)
                {
                    return Reply(
                        notAuthor ? "A user cannot inherit from himself." : "You cannot inherit from yourself"
                    );
                }

                List<SbuMember> result;

                await using (Context.BeginYield())
                {
                    result = await Context.Db.Members
                        .Include(m => m.ColorRole)
                        .Where(m => m.DiscordId == deceasedId || m.DiscordId == inheritor.Id)
                        .ToListAsync();
                }

                if (result.FirstOrDefault(m => m.DiscordId == deceasedId) is not { } deceasedMember)
                    return Reply("Could not find the deceased member in the database.");

                if (result.FirstOrDefault(m => m.DiscordId == inheritor.Id) is { } inheritorMember)
                    Context.Db.Remove(inheritorMember);

                deceasedMember.DiscordId = inheritor.Id;

                await using (Context.BeginYield())
                {
                    await Context.Db.SaveChangesAsync();
                }

                if (deceasedMember.ColorRole is { }
                    && Context.Guild.Roles.ContainsKey(deceasedMember.ColorRole.DiscordId)
                )
                {
                    if (Context.CurrentMember.GetGuildPermissions().ManageRoles)
                    {
                        try
                        {
                            await Context.Guild.GrantRoleAsync(inheritor.Id, deceasedMember.ColorRole.DiscordId);
                        }
                        catch (Exception ex)
                        {
                            Context.Bot.Logger.LogWarning(ex, "Could not assign role in {Command}", Context.Path);
                            await Reply("Could not assign color role because of an internal error.");
                        }
                    }
                    else
                    {
                        await Reply("Could not assign color role, missing permissions.");
                    }
                }

                return Reply(
                    $"{(notAuthor ? inheritor.Mention : "You")} inherited all of {Mention.User(deceasedId)}'s owned "
                    + "database entries."
                );
            }
        }

        [Group("claim", "take"), RequireAuthorInDb]
        public sealed class ClaimGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ClaimColorRoleAsync(SbuColorRole colorRole)
            {
                if (colorRole.OwnerId is { })
                {
                    return Reply(
                        $"The given tag cannot be claimed, it is owned by {Mention.User(colorRole.OwnerId.Value)}."
                    );
                }

                await using (Context.BeginYield())
                {
                    colorRole.OwnerId = Context.Author.Id;
                    Context.Db.ColorRoles.Update(colorRole);
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"You now own {Mention.Role(colorRole.DiscordId)}.");
            }

            [Command, Priority(-1)]
            public async Task<DiscordCommandResult> ClaimNewColorRoleAsync(
                [MustBeColorRole, MustExistInDb(false)]
                IRole colorRole
            )
            {
                SbuColorRole? dbRole;

                await using (Context.BeginYield())
                {
                    dbRole = await Context.Db.ColorRoles.Include(r => r.Owner)
                        .FirstOrDefaultAsync(m => m.DiscordId == colorRole.Id);
                }

                if (dbRole is { })
                    return Reply($"This role is already owned by {Mention.User(dbRole.Owner!.DiscordId)}.");

                await using (Context.BeginYield())
                {
                    Context.Db.ColorRoles.Add(new(colorRole, Context.Author.Id));
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"You now own {colorRole.Mention}.");
            }

            [Command]
            public async Task<DiscordCommandResult> ClaimTagAsync(SbuTag tag)
            {
                if (tag.OwnerId is { })
                    return Reply($"The given tag cannot be claimed, it is owned by {Mention.User(tag.OwnerId.Value)}.");

                await using (Context.BeginYield())
                {
                    tag.OwnerId = Context.Author.Id;
                    Context.Db.Tags.Update(tag);
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"You now own `{tag.Name}`.");
            }
        }

        [Group("transfer", "gift"), RequireAuthorInDb]
        public sealed class TransferGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> TransferColorRoleAsync(
                [NotAuthor, MustExistInDb] IMember member,
                [AuthorMustOwn] SbuColorRole role
            )
            {
                await using (Context.BeginYield())
                {
                    role.OwnerId = member.Id;
                    Context.Db.ColorRoles.Update(role);
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"{member.Mention} now owns {Mention.Role(role.DiscordId)}.");
            }

            [Command]
            public async Task<DiscordCommandResult> TransferTagAsync(
                [NotAuthor, MustExistInDb] IMember member,
                [AuthorMustOwn] SbuTag tag
            )
            {
                await using (Context.BeginYield())
                {
                    tag.OwnerId = member.Id;
                    Context.Db.Tags.Update(tag);
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"{member.Mention} now owns `{tag.Name}`.");
            }

            [Command("all"), Disabled]
            public async Task<DiscordCommandResult> TransferAllAsync([NotAuthor, MustExistInDb] IMember member)
            {
                // TODO: either do manually or use direct query
                await using (Context.BeginYield())
                {
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"{member.Mention} now owns all of {Context.Author.Mention}'s db entries.");
            }
        }
    }
}