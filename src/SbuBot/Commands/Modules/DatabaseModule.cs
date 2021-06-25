using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
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
            SbuDbContext dbContext = Context.GetSbuDbContext();
            SbuGuild guild = await dbContext.GetGuildAsync(Context.Guild);
            SbuMember newMember = new(member, guild.Id);

            if (member.GetColorRole() is { } colorRole)
                dbContext.ColorRoles.Add(new(colorRole, guild.Id, newMember.Id));

            dbContext.Members.Add(newMember);
            await dbContext.SaveChangesAsync();

            return Reply($"{member.Mention} is now registered in the database.");
        }

        [Command("init"), RequireBotOwner]
        [Description("Initializes the database for this guild, loading members and color roles into it.")]
        public async Task<DiscordCommandResult> InitAsync()
        {
            SbuDbContext dbContext = Context.GetSbuDbContext();
            int userCount = 0, roleCount = 0;

            IEnumerable<(IMember m, IRole?)> userRolePairs = Context.Guild.Members.Values
                .Where(m => !m.IsBot)
                .Select(m => (m, m.GetColorRole()));

            SbuGuild guild = await dbContext.GetGuildAsync(Context.Guild);

            // BUG: does not insert anything
            // TODO: prefetch by guildId
            foreach ((IMember member, IRole? role) in userRolePairs)
            {
                if (await dbContext.GetMemberAsync(Context.Author) is not { } dbMember)
                {
                    dbMember = new(member, guild.Id);
                    dbContext.Members.Add(dbMember);
                }

                userCount++;

                if (role is null)
                    continue;

                if (await dbContext.GetColorRoleAsync(role) is { } dbRole)
                {
                    dbRole.OwnerId = dbMember.Id;
                    dbContext.ColorRoles.Update(dbRole);
                }
                else
                {
                    dbContext.ColorRoles.Add(new(role, dbMember.Id, guild.Id));
                }

                roleCount++;
            }

            await dbContext.SaveChangesAsync();
            return Reply($"Found {userCount} users, {roleCount} of which have a suitable color role.");
        }

        [Command("transfer"), RequireAdmin]
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

            List<SbuTag> tags = await dbContext
                .Tags
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
                    ConsistencyService service = Context.Services.GetRequiredService<ConsistencyService>();
                    service.IgnoreAddedRole(role.Id);

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

        [Group("inspect"), RequireBotOwner]
        [Description("Inspects a given entity's database entry.")]
        public sealed class InspectGroup : SbuModuleBase
        {
            [Command]
            public DiscordCommandResult InspectMember(
                [Description("The member to inspect.")]
                SbuMember member,
                [Description("Additional members to inspect.")]
                params SbuMember[] additionalMembers
            ) => Reply(
                additionalMembers.Prepend(member)
                    .Select(
                        m => new LocalEmbed()
                            .WithAuthor(Context.Guild.GetMember(m.Id))
                            .WithDescription(Markdown.CodeBlock("yml", m))
                            .AddInlineField("Self", Mention.User(m.Id))
                            .AddInlineField(
                                "ColorRole",
                                m.ColorRole is { } ? Mention.Role(m.ColorRole.Id) : "None"
                            )
                    )
                    .ToArray()
            );

            [Command]
            public DiscordCommandResult InspectRole(
                [Description("The role to inspect.")] SbuColorRole role,
                [Description("Additional roles to inspect.")]
                params SbuColorRole[] additionalRoles
            ) => Reply(
                additionalRoles.Prepend(role)
                    .Select(
                        r =>
                        {
                            LocalEmbed embed = new LocalEmbed()
                                .WithTitle("Role")
                                .WithDescription(Markdown.CodeBlock("yml", r))
                                .AddInlineField("Self", Mention.Role(r.Id))
                                .AddInlineField(
                                    "Owner",
                                    r.OwnerId is { } ? Mention.User(r.OwnerId.Value) : "None"
                                );

                            if (r.OwnerId is { })
                                embed.WithAuthor(Context.Guild.GetMember(r.OwnerId.Value));

                            return embed;
                        }
                    )
                    .ToArray()
            );

            [Command]
            public DiscordCommandResult InspectTag(
                [Description("The tag to inspect.")] SbuTag tag,
                [Description("Additional tags to inspect.")]
                params SbuTag[] additionalTags
            ) => Reply(
                additionalTags.Prepend(tag)
                    .Select(
                        t =>
                        {
                            LocalEmbed embed = new LocalEmbed()
                                .WithTitle("Tag")
                                .WithDescription(Markdown.CodeBlock("yml", t))
                                .AddInlineField("Owner", t.OwnerId is { } ? Mention.User(t.OwnerId.Value) : "None");

                            if (t.OwnerId is { })
                                embed.WithAuthor(Context.Guild.GetMember(t.OwnerId.Value));

                            return embed;
                        }
                    )
                    .ToArray()
            );

            [Command]
            public DiscordCommandResult InspectTag(
                [Description("The reminder to inspect.")]
                SbuReminder reminder,
                [Description("Additional reminders to inspect.")]
                params SbuReminder[] additionalReminders
            ) => Reply(
                additionalReminders.Prepend(reminder)
                    .Select(
                        r => new LocalEmbed()
                            .WithAuthor(Context.Guild.GetMember(r.OwnerId.Value))
                            .WithTitle("Reminder")
                            .WithDescription(Markdown.CodeBlock("yml", r))
                            .AddInlineField("Owner", Mention.User(r.OwnerId.Value))
                            .AddInlineField("CreatedAt", r.CreatedAt)
                            .AddInlineField("DueAt", r.DueAt)
                    )
                    .ToArray()
            );
        }
    }
}