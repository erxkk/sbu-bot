using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Commands.Information;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("db")]
    [Description("A collection of commands for managing the database.")]
    public sealed class DatabaseModule : SbuModuleBase
    {
        [Command("register"), RequireBotOwner]
        [Description("Registers a member and their color role in the database.")]
        public async Task<DiscordCommandResult> RegisterAsync(
            [NotAuthor, MustExistInDb(false)][Description("The member to register in the database.")]
            IMember member
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
        [Description("Initializes the database, loading members and color roles into it.")]
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

        [Command("transfer"), RequireAuthorAdmin]
        [Description("Transfers a members database entries to another member.")]
        public async Task<DiscordCommandResult> TransferAllAsync(
            [Description("The member that owns the database entries.")]
            SbuMember owner,
            [Description("The member that should receive the database entries.")]
            SbuMember receiver
        )
        {
            if (owner.DiscordId == receiver.DiscordId)
                return Reply("The given members cannot be the same");

            List<SbuTag> tags;

            await using (Context.BeginYield())
            {
                tags = await Context.Db.Tags.Where(t => t.OwnerId == owner.DiscordId).ToListAsync();
            }

            foreach (SbuTag tag in tags)
            {
                tag.OwnerId = receiver.DiscordId;
            }

            bool hadRole = false;

            if (owner.ColorRole is { })
            {
                hadRole = true;
                SbuColorRole role = Context.Invoker.ColorRole!;

                if (receiver.ColorRole is null)
                {
                    ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
                    service.IgnoreAddedRole(role.DiscordId);

                    await Context.Guild.GrantRoleAsync(receiver.DiscordId, role.DiscordId);

                    role.OwnerId = receiver.DiscordId;
                }
                else
                {
                    role.OwnerId = null;
                }

                Context.Db.ColorRoles.Update(role);
                await Context.Author.RevokeRoleAsync(role.DiscordId);
            }

            if (tags.Count != 0 || hadRole)
                await Context.Db.SaveChangesAsync();

            return Reply(
                string.Format(
                    "{0} now owns all of {1}'s db entries.",
                    Mention.User(receiver.DiscordId),
                    Mention.User(owner.DiscordId)
                )
            );
        }

        [Group("inspect"), PureGroup, RequireBotOwner]
        [Description("Inspects a given entity's database entry.")]
        public sealed class InspectGroup : SbuModuleBase
        {
            [Command]
            public DiscordCommandResult InspectMember(
                [Description("The member to inspect.")]
                SbuMember member
            ) => Reply(
                new LocalEmbed()
                    .WithTitle($"Member : ({member.DiscordId}) {member.Id}")
                    .WithDescription(member.ToString())
            );

            [Command]
            public DiscordCommandResult InspectRole(
                [Description("The role to inspect.")] SbuColorRole role
            ) => Reply(
                new LocalEmbed()
                    .WithTitle($"Role : ({role.DiscordId}) {role.Id}")
                    .WithDescription(role.ToString())
                    .AddField("Owner", role.OwnerId is { } ? Mention.User(role.OwnerId.Value) : "None")
            );

            [Command]
            public DiscordCommandResult InspectTag(
                [Description("The tag to inspect.")] SbuTag tag
            ) => Reply(
                new LocalEmbed()
                    .WithTitle($"Tag : {tag.Id}")
                    .WithDescription($"Name: {tag.Name}\n{Markdown.CodeBlock(tag.Content)}")
                    .AddField("Owner", tag.OwnerId is { } ? Mention.User(tag.OwnerId.Value) : "None")
            );

            [Command]
            public DiscordCommandResult InspectTag(
                [Description("The reminder to inspect.")]
                SbuReminder reminder
            ) => Reply(
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