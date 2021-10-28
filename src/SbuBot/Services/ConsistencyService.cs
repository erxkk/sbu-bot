using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Exceptions;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ConsistencyService : DiscordBotService
    {
        private readonly HashSet<Snowflake> _handledAddedRoles = new();
        private readonly HashSet<Snowflake> _handledRemovedRoles = new();

        public override int Priority => int.MaxValue;

        public void IgnoreAddedRole(Snowflake roleId) => _handledAddedRoles.Add(roleId);
        public void IgnoreRemovedRole(Snowflake roleId) => _handledRemovedRoles.Add(roleId);

        // TODO: clean this up, idk why the guild wasn't added with the last build
        private async ValueTask _TryAddGuild(IGuild guild)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetGuildAsync(guild.Id) is null)
                {
                    context.AddGuild(guild);
                    await context.SaveChangesAsync();

                    Logger.LogDebug("Guild inserted: {@Guild}", new { guild.Id });
                }
            }
        }

        protected override ValueTask OnJoinedGuild(JoinedGuildEventArgs e) => _TryAddGuild(e.Guild);

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            await _TryAddGuild(e.Guild);

            if (e.GuildId == SbuGlobals.Guild.SBU || e.GuildId == SbuGlobals.Guild.LA_FAMILIA)
                await Bot.Chunker.ChunkAsync(e.Guild);
        }

        protected override async ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                ReminderService service = scope.ServiceProvider.GetRequiredService<ReminderService>();

                if (await context.GetGuildAsync(e.GuildId) is not { } guild)
                    return;

                context.Guilds.Remove(guild);
                await service.CancelAsync(r => r.GuildId == guild.Id);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Guild removed: {@Guild}", new { Id = e.GuildId });
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetMemberAsync(e.Member.Id, e.GuildId) is { })
                    return;

                context.AddMember(e.Member);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Member inserted: {@Member}", new { e.Member.Id, Guild = e.GuildId });
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            if (e.NewMember.IsBot)
                return;

            if (e.OldMember is null)
                throw new NotCachedException("Could not find required member in cache.");

            if (e.OldMember.RoleIds.Count == e.NewMember.RoleIds.Count)
                return;

            IEnumerable<Snowflake> except = e.NewMember.RoleIds.Except(e.OldMember.RoleIds);

            if (except.Count() != 1)
                return;

            Snowflake addedRoleId = except.First();

            if (Bot.GetRole(e.NewMember.GuildId, addedRoleId) is not { } addedRole)
                throw new NotCachedException("Could not find required role in cache.");

            if (addedRole.Position >= Bot.GetColorRoleSeparator().Position)
                return;

            if (_handledAddedRoles.Remove(addedRoleId))
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.Guilds.FirstOrDefaultAsync(g => g.Id == e.NewMember.GuildId) is null)
                    return;

                if (await context.GetMemberAsync(e.NewMember) is not { } member)
                {
                    member = new(e.NewMember);
                    context.Members.Add(member);
                }

                if (await context.GetColorRoleAsync(addedRole) is not { } role)
                {
                    role = new(addedRole, member.Id);
                    context.ColorRoles.Add(role);
                }
                else
                {
                    role.OwnerId = member.Id;
                }

                await context.SaveChangesAsync();

                Logger.LogDebug(
                    "Color role assigned, established owner ship: {@ColorRole}",
                    new { Id = addedRoleId, Guild = e.NewMember.GuildId, Owner = e.MemberId }
                );
            }
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            if (e.User.IsBot)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                ReminderService service = scope.ServiceProvider.GetRequiredService<ReminderService>();

                if (await context.GetMemberAsync(e.User.Id, e.GuildId) is not { } member)
                    return;

                await service.CancelAsync(r => r.OwnerId == member.Id);
                await context.SaveChangesAsync();
                Logger.LogDebug("Member removed: {@Member}", new { e.User.Id, Guild = e.GuildId });
            }
        }

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            if (e.GuildId is null)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetMemberAsync(e.Member.Id, e.GuildId.Value) is { })
                    return;

                context.Members.Add(new(e.Member));
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Member inserted: {@Member}", new { e.Member.Id, Guild = e.GuildId });
        }

        protected override async ValueTask OnRoleDeleted(RoleDeletedEventArgs e)
        {
            if (e.Role.Position >= Bot.GetColorRoleSeparator().Position)
                return;

            if (_handledRemovedRoles.Remove(e.RoleId))
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetColorRoleAsync(e.RoleId, e.GuildId) is { } role)
                {
                    context.ColorRoles.Remove(role);
                    await context.SaveChangesAsync();
                }
            }

            Logger.LogDebug("Role removed: {@ColorRole}", new { Id = e.RoleId, Guild = e.GuildId });
        }
    }
}