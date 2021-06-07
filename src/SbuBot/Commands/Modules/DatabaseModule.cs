using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Commands.Information;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("db")]
    public sealed class DatabaseModule : SbuModuleBase
    {
        [Command("register"), RequireBotOwner]
        public async Task<DiscordCommandResult> RegisterMemberAsync(
            [NotAuthor, MustExistInDb(false)] IMember member
        )
        {
            SbuMember newMember = new(member);

            if (Utility.GetSbuColorRole(member) is { } colorRole)
                Context.Db.ColorRoles.Add(new(colorRole, newMember.DiscordId));

            await using (Context.BeginYield())
            {
                Context.Db.Members.Add(newMember);
                await Context.Db.SaveChangesAsync();
            }

            return Reply($"{member.Mention} is now registered in the database.");
        }

        [Command("init"), RequireBotOwner]
        public async Task<DiscordCommandResult> InitAsync()
        {
            int userCount = 0, roleCount = 0;

            IEnumerable<(IMember m, IRole?)> userRolePairs = Context.Guild.Members.Values
                .Where(m => !m.IsBot)
                .Select(m => (m, Utility.GetSbuColorRole(m)));

            foreach ((IMember member, IRole? role) in userRolePairs)
            {
                Context.Db.Members.Add(new(member));
                userCount++;

                if (role is null)
                    continue;

                Context.Db.ColorRoles.Add(new(role, member.Id));
                roleCount++;
            }

            await Context.Db.SaveChangesAsync();
            return Reply($"Found {userCount} users, {roleCount} of which have a suitable color role.");
        }

        // TODO: TEST
        [Group("transfer"), PureGroup]
        public sealed class TransferGroup : SbuModuleBase
        {
            [Command, RequireAuthorAdmin]
            public async Task<DiscordCommandResult> TransferAllAsync(SbuMember owner, [NotAuthor] SbuMember inheritor)
            {
                List<SbuTag> tags;

                await using (Context.BeginYield())
                {
                    tags = await Context.Db.Tags.Where(t => t.OwnerId == owner.DiscordId).ToListAsync();
                }

                foreach (SbuTag tag in tags)
                {
                    tag.OwnerId = inheritor.DiscordId;
                }

                owner.ColorRole!.OwnerId = inheritor.DiscordId;
                Context.Db.Tags.UpdateRange(tags);
                Context.Db.ColorRoles.Update(owner.ColorRole);
                await Context.Db.SaveChangesAsync();

                await Context.Guild.GrantRoleAsync(inheritor.DiscordId, owner.ColorRole!.DiscordId);

                return Reply(
                    string.Format(
                        "{0} now owns all of {1}'s db entries.",
                        Mention.User(inheritor.DiscordId),
                        Mention.User(owner.DiscordId)
                    )
                );
            }

            [Command]
            public async Task<DiscordCommandResult> AuthorTransferAllAsync([NotAuthor] SbuMember member)
            {
                List<SbuTag> tags;

                await using (Context.BeginYield())
                {
                    tags = await Context.Db.Tags.Where(t => t.OwnerId == Context.Author.Id).ToListAsync();
                }

                foreach (SbuTag tag in tags)
                {
                    tag.OwnerId = member.DiscordId;
                }

                Context.Invoker!.ColorRole!.OwnerId = member.DiscordId;
                Context.Db.Tags.UpdateRange(tags);
                Context.Db.ColorRoles.Update(Context.Invoker!.ColorRole);
                await Context.Db.SaveChangesAsync();

                await Context.Author.GrantRoleAsync(Context.Invoker!.ColorRole!.DiscordId);

                return Reply(
                    $"{Mention.User(member.DiscordId)} now owns all of {Context.Author.Mention}'s db entries."
                );
            }
        }

        [Group("inspect"), PureGroup, RequireBotOwner]
        public sealed class InspectGroup : SbuModuleBase
        {
            [Command]
            public DiscordCommandResult InspectMember(SbuMember member) => Reply(
                new LocalEmbed()
                    .WithTitle($"Member : ({member.DiscordId}) {member.Id}")
                    .WithDescription(member.ToString())
            );

            [Command]
            public DiscordCommandResult InspectRole(SbuColorRole role) => Reply(
                new LocalEmbed()
                    .WithTitle($"Role : ({role.DiscordId}) {role.Id}")
                    .WithDescription(role.ToString())
                    .AddField("Owner", role.OwnerId is { } ? Mention.User(role.OwnerId.Value) : "None")
            );

            [Command]
            public DiscordCommandResult InspectTag(SbuTag tag) => Reply(
                new LocalEmbed()
                    .WithTitle($"Tag : {tag.Id}")
                    .WithDescription($"Name: {tag.Name}\n{Markdown.CodeBlock(tag.Content)}")
                    .AddField("Owner", tag.OwnerId is { } ? Mention.User(tag.OwnerId.Value) : "None")
            );

            [Command]
            public DiscordCommandResult InspectTag(SbuReminder reminder) => Reply(
                new LocalEmbed()
                    .WithTitle($"Reminder : {reminder.Id}")
                    .WithDescription("\nreminder.Message")
                    .AddField("Owner", Mention.User(reminder.OwnerId.Value), true)
                    .AddField("CreatedAt", reminder.CreatedAt, true)
                    .AddField("DueAt", reminder.DueAt, true)
            );
        }
    }
}