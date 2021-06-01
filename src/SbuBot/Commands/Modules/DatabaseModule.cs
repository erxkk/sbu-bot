using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    [Group("db")]
    public sealed class DatabaseModule : SbuModuleBase
    {
        [Group("register")]
        public sealed  class RegisterGroup : SbuModuleBase
        {
            [Command, RequireAuthorInDb(false)]
            public async Task<DiscordCommandResult> RegisterAuthorAsync()
                => await _registerMemberOrAuthorAsync(Context.Author);

            [Command, RequireAuthorAdmin]
            public async Task<DiscordCommandResult> RegisterMemberAsync(
                [NotAuthor, MustExistInDb(false)] IMember member
            ) => await _registerMemberOrAuthorAsync(member);

            private async Task<DiscordCommandResult> _registerMemberOrAuthorAsync(IMember member)
            {
                bool notAuthor = member.Id != Context.Author.Id;

                if (Context.Invoker is { })
                {
                    return Reply(
                        $"{(notAuthor ? "This member already exists" : "You already exist")} in the database."
                    );
                }

                SbuMember newMember = new(member);

                if (Utility.GetSbuColorRole(member) is { } colorRole)
                    Context.Db.ColorRoles.Add(new(colorRole, newMember.DiscordId));

                await using (Context.BeginYield())
                {
                    Context.Db.Members.Add(newMember);
                    await Context.Db.SaveChangesAsync();
                }

                return Reply($"{(notAuthor ? "This member is" : "You are")} now registered in the database.");
            }
        }

        [Group("show"), RequireAuthorAdmin]
        public sealed class ShowGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ShowDbEntityAsync(DbEntityType entityType, Guid id)
            {
                (IQueryable<SbuEntityBase> queryable, string entityName) = (entityType switch
                {
                    DbEntityType.Member => (Context.Db.Members as IQueryable<SbuEntityBase>, "Member"),
                    DbEntityType.Role => (Context.Db.ColorRoles, "Role"),
                    DbEntityType.Tag => (Context.Db.Tags, "Tag"),
                    DbEntityType.Reminder => (Context.Db.Reminders, "Reminder"),
                    _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null),
                });

                SbuEntityBase? entity;

                await using (Context.BeginYield())
                {
                    entity = await queryable.FirstOrDefaultAsync(e => e.Id == id);
                }

                if (entity is null)
                    return Reply("Could not find the given entity.");

                return Reply(
                    new LocalEmbedBuilder()
                        .WithTitle($"{entityName} : {entity.Id}")
                        .WithDescription(entity.ToString())
                );
            }

            [Command]
            public async Task<DiscordCommandResult> ShowDiscordEntityAsync(DbDiscordEntityType entityType, Snowflake id)
            {
                (IQueryable<ISbuDiscordEntity> queryable, string entityName) = (entityType switch
                {
                    DbDiscordEntityType.Member => (Context.Db.Members as IQueryable<ISbuDiscordEntity>, "Member"),
                    DbDiscordEntityType.Role => (Context.Db.ColorRoles, "Role"),
                    _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null),
                });

                ISbuDiscordEntity? entity;

                await using (Context.BeginYield())
                {
                    entity = await queryable.FirstOrDefaultAsync(e => e.DiscordId == id);
                }

                if (entity is null)
                    return Reply("Could not find the given entity.");

                return Reply(
                    new LocalEmbedBuilder()
                        .WithTitle($"{entityName} : ({entity.DiscordId}) {entity.Id}")
                        .WithDescription(entity.ToString())
                );
            }

            [Command]
            public async Task<DiscordCommandResult> ShowMemberAsync([NotAuthor] IMember member)
            {
                ISbuDiscordEntity? dbMember;

                await using (Context.BeginYield())
                {
                    dbMember = await Context.Db.Members.FirstOrDefaultAsync(e => e.DiscordId == member.Id);
                }

                if (dbMember is null)
                    return Reply("Could not find the given member.");

                return Reply(
                    new LocalEmbedBuilder()
                        .WithTitle($"Member : ({dbMember.DiscordId}) {dbMember.Id}")
                        .WithDescription(dbMember.ToString())
                );
            }

            [Command]
            public async Task<DiscordCommandResult> ShowRoleAsync(IRole role)
            {
                ISbuDiscordEntity? dbRole;

                await using (Context.BeginYield())
                {
                    dbRole = await Context.Db.ColorRoles.FirstOrDefaultAsync(e => e.DiscordId == role.Id);
                }

                if (dbRole is null)
                    return Reply("Could not find the given role.");

                return Reply(
                    new LocalEmbedBuilder()
                        .WithTitle($"Role : ({dbRole.DiscordId}) {dbRole.Id}")
                        .WithDescription(dbRole.ToString())
                );
            }
        }

        public enum DbEntityType
        {
            Member,
            Role,
            Tag,
            Reminder,
        }

        public enum DbDiscordEntityType
        {
            Member,
            Role,
        }
    }
}