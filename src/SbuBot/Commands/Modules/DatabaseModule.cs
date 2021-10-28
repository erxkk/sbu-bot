using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("db")]
    [Description("A collection of commands for managing the database.")]
    public sealed class DatabaseModule : SbuModuleBase
    {
        [Command("register")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [Description("Registers a member and their color role in the database.")]
        public async Task<DiscordCommandResult> RegisterAsync(
            [NotAuthor, MustExistInDb(false)][Description("The member to register in the database.")]
            IMember member
        )
        {
            SbuDbContext dbContext = Context.GetSbuDbContext();
            SbuMember newMember = dbContext.AddMember(member);

            if (member.GetColorRole() is { } colorRole)
                dbContext.AddColorRole(colorRole, newMember.Id);

            await dbContext.SaveChangesAsync();

            return Reply($"{member.Mention} is now registered in the database.");
        }

        [Command("init")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [Description("Initializes the database for this guild, loading members and color roles into it.")]
        public async Task<DiscordCommandResult> InitAsync()
        {
            SbuDbContext dbContext = Context.GetSbuDbContext();
            int userCount = 0, roleCount = 0;

            IEnumerable<(IMember m, IRole?)> userRolePairs = Context.Guild.Members.Values
                .Where(m => !m.IsBot)
                .Select(m => (m, m.GetColorRole()));

            Dictionary<Snowflake, SbuMember> members = await dbContext.Members
                .Where(m => m.GuildId == Context.Guild.Id)
                .ToDictionaryAsync(k => k.Id, v => v, Context.Bot.StoppingToken);

            Dictionary<Snowflake, SbuColorRole> colorRoles = await dbContext.ColorRoles
                .Where(cr => cr.GuildId == Context.Guild.Id)
                .ToDictionaryAsync(k => k.Id, v => v, Context.Bot.StoppingToken);

            foreach ((IMember member, IRole? role) in userRolePairs)
            {
                if (!members.TryGetValue(member.Id, out var dbMember))
                    dbMember = dbContext.AddMember(member);

                userCount++;

                if (role is null)
                    continue;

                if (colorRoles.TryGetValue(role.Id, out var dbRole))
                {
                    dbRole.OwnerId = dbMember.Id;
                    dbContext.ColorRoles.Update(dbRole);
                }
                else
                {
                    dbContext.AddColorRole(role, dbMember.Id);
                }

                roleCount++;
            }

            await dbContext.SaveChangesAsync();
            return Reply($"Found {userCount} users, {roleCount} of which have a suitable color role.");
        }

        [Command("transfer")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [Description("Transfers a members database entries to another member.")]
        public async Task<DiscordCommandResult> TransferAllAsync(
            [Description("The member that owns the database entries.")]
            SbuMember owner,
            [Description("The member that should receive the database entries.")]
            SbuMember receiver
        )
        {
            SbuDbContext dbContext = Context.GetSbuDbContext();

            if (owner.Id == receiver.Id)
                return Reply("The given members cannot be the same.");

            List<SbuTag> tags = await dbContext.Tags
                .Where(t => t.OwnerId == owner.Id)
                .ToListAsync(Context.Bot.StoppingToken);

            foreach (SbuTag tag in tags)
                tag.OwnerId = receiver.Id;

            bool hadRole = false;

            if (owner.ColorRole is { })
            {
                hadRole = true;
                SbuColorRole role = owner.ColorRole;

                if (receiver.ColorRole is null)
                {
                    await Context.Guild.GrantRoleAsync(receiver.Id, role.Id);

                    role.OwnerId = receiver.Id;
                }
                else
                {
                    role.OwnerId = null;
                }

                dbContext.ColorRoles.Update(role);
                await Context.Author.RevokeRoleAsync(role.Id);
            }

            if (tags.Count != 0 || hadRole)
                await dbContext.SaveChangesAsync();

            return Reply(
                string.Format(
                    "{0} now owns all of {1}'s db entries.",
                    Mention.User(receiver.Id),
                    Mention.User(owner.Id)
                )
            );
        }

        [Group("inspect")]
        [RequireBotOwner]
        [Description("Inspects a given entity's database entry.")]
        public sealed class InspectGroup : SbuModuleBase
        {
            [Command]
            public DiscordCommandResult InspectMember(
                [Description("The member to inspect.")]
                SbuMember member
            ) => Reply(
                new LocalEmbed()
                    .WithAuthor(Context.Guild.GetMember(member.Id))
                    .WithDescription(Markdown.CodeBlock("yml", member.GetInspection()))
                    .AddInlineField("Self", Mention.User(member.Id))
                    .AddInlineField("ColorRole", member.ColorRole is { } ? Mention.Role(member.ColorRole.Id) : "None")
            );

            [Command]
            public DiscordCommandResult InspectRole([Description("The role to inspect.")] SbuColorRole role)
            {
                LocalEmbed embed = new LocalEmbed()
                    .WithTitle("Role")
                    .WithDescription(Markdown.CodeBlock("yml", role.GetInspection()))
                    .AddInlineField("Self", Mention.Role(role.Id))
                    .AddInlineField("Owner", role.OwnerId is { } ? Mention.User(role.OwnerId.Value) : "None");

                if (role.OwnerId is { })
                    embed.WithAuthor(Context.Guild.GetMember(role.OwnerId.Value));

                return Reply(embed);
            }

            [Command]
            public DiscordCommandResult InspectTag(
                [Description("The tag to inspect.")] SbuTag tag
            )
            {
                LocalEmbed embed = new LocalEmbed()
                    .WithTitle("Tag")
                    .WithDescription(Markdown.CodeBlock("yml", tag.GetInspection(3)))
                    .AddInlineField("Owner", tag.OwnerId is { } ? Mention.User(tag.OwnerId.Value) : "None");

                if (tag.OwnerId is { })
                    embed.WithAuthor(Context.Guild.GetMember(tag.OwnerId.Value));

                return Reply(embed);
            }

            [Command]
            public DiscordCommandResult InspectAutoResponse(
                [Description("The auto response to inspect.")]
                SbuAutoResponse autoResponse
            ) => Reply(
                new LocalEmbed()
                    .WithTitle("AutoResponse")
                    .WithDescription(Markdown.CodeBlock("yml", autoResponse.GetInspection(3)))
            );

            [Command]
            public DiscordCommandResult InspectReminder(
                [Description("The reminder to inspect.")]
                SbuReminder reminder
            ) => Reply(
                new LocalEmbed()
                    .WithAuthor(Context.Guild.GetMember(reminder.OwnerId.Value))
                    .WithTitle("Reminder")
                    .WithDescription(Markdown.CodeBlock("yml", reminder.GetInspection()))
                    .AddInlineField("Owner", Mention.User(reminder.OwnerId.Value))
                    .AddInlineField("CreatedAt", reminder.CreatedAt)
                    .AddInlineField("DueAt", reminder.DueAt)
            );
        }
    }
}