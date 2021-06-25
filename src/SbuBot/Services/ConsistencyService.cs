using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
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

        public ConsistencyService(ILogger<ConsistencyService> logger, DiscordBotBase bot) : base(logger, bot) { }

        public void IgnoreAddedRole(Snowflake roleId) => _handledAddedRoles.Add(roleId);
        public void IgnoreRemovedRole(Snowflake roleId) => _handledRemovedRoles.Add(roleId);

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.Guilds.FirstOrDefaultAsync(
                    g => g.Id == e.GuildId,
                    Bot.StoppingToken
                ) is null)
                {
                    context.Guilds.Add(new(e.Guild));
                    await context.SaveChangesAsync(Bot.StoppingToken);
                }
            }

            await Bot.Chunker.ChunkAsync(e.Guild, Bot.StoppingToken);
            await base.OnGuildAvailable(e);
        }

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.Guilds.FirstOrDefaultAsync(
                    g => g.Id == e.GuildId,
                    Bot.StoppingToken
                ) is null)
                {
                    context.Guilds.Add(new(e.Guild));
                    await context.SaveChangesAsync(Bot.StoppingToken);
                }
            }

            Logger.LogDebug("Guild inserted: {@Guild}", new { Id = e.GuildId });
            await base.OnJoinedGuild(e);
        }

        protected override async ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                ReminderService service = scope.ServiceProvider.GetRequiredService<ReminderService>();

                if (await context.Guilds.FirstOrDefaultAsync(g => g.Id == e.GuildId) is not { } guild)
                    return;

                context.ColorRoles.RemoveRange(
                    await context.ColorRoles
                        .Where(t => t.GuildId == guild.Id)
                        .ToListAsync(Bot.StoppingToken)
                );

                context.Tags.RemoveRange(
                    await context.Tags
                        .Where(t => t.GuildId == guild.Id)
                        .ToListAsync(Bot.StoppingToken)
                );

                await service.CancelAsync(q => q.Where(r => r.Value.GuildId == guild.Id));
                await context.SaveChangesAsync(Bot.StoppingToken);
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

                if (await context.Guilds.FirstOrDefaultAsync(g => g.Id == e.GuildId) is not { } guild)
                    return;

                if (await context.Members.FirstOrDefaultAsync(
                    m => m.Id == e.Member.Id && m.GuildId == guild.Id,
                    Bot.StoppingToken
                ) is { }) return;

                context.Members.Add(new(e.Member, guild.Id));
                await context.SaveChangesAsync(Bot.StoppingToken);
            }

            Logger.LogDebug("Member inserted: {@Member}", new { e.Member.Id, Guild = e.GuildId });
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            if (e.NewMember.IsBot)
                return;

            if (e.OldMember is null)
                return;

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

                if (await context.Guilds.FirstOrDefaultAsync(g => g.Id == e.NewMember.GuildId) is not { } guild)
                {
                    await base.OnMemberUpdated(e);
                    return;
                }

                if (await context.Members.FirstOrDefaultAsync(
                    m => m.Id == e.MemberId && m.GuildId == guild.Id,
                    Bot.StoppingToken
                ) is not { } member)
                {
                    member = new(e.NewMember, guild.Id);
                    context.Members.Add(member);
                }

                if (await context.ColorRoles.FirstOrDefaultAsync(r => r.GuildId == guild.Id) is { } role)
                {
                    role.OwnerId = member.Id;
                }
                else
                {
                    role = new(addedRole, member.Id, guild.Id);
                    context.ColorRoles.Add(role);
                }

                await context.SaveChangesAsync(Bot.StoppingToken);

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

                if (await context.Guilds.FirstOrDefaultAsync(g => g.Id == e.GuildId) is not { } guild)
                    return;

                if (await context.Members.FirstOrDefaultAsync(
                    m => m.Id == e.User.Id && m.GuildId == guild.Id,
                    Bot.StoppingToken
                ) is not { } member) return;

                if (await context.ColorRoles.FirstOrDefaultAsync(
                    m => m.OwnerId == member.Id && m.GuildId == guild.Id,
                    Bot.StoppingToken
                ) is { } role)
                {
                    role.OwnerId = null;
                    context.ColorRoles.Update(role);
                }

                List<SbuTag> tags = await context.Tags
                    .Where(t => t.OwnerId == member.Id && t.GuildId == guild.Id)
                    .ToListAsync(Bot.StoppingToken);

                foreach (SbuTag tag in tags)
                    tag.OwnerId = null;

                context.Tags.UpdateRange(tags);

                await service.CancelAsync(q => q.Where(r => r.Value.OwnerId == member.Id));
                await context.SaveChangesAsync(Bot.StoppingToken);
                Logger.LogDebug("Member removed: {@Member}", new { e.User.Id, Guild = e.GuildId });
            }
        }

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.Guilds.FirstOrDefaultAsync(g => g.Id == e.GuildId) is not { } guild)
                    return;

                if (await context.Members.FirstOrDefaultAsync(
                    m => m.Id == e.Member.Id && m.GuildId == guild.Id,
                    Bot.StoppingToken
                ) is { }) return;

                context.Members.Add(new(e.Member, guild.Id));
                await context.SaveChangesAsync(Bot.StoppingToken);
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

                if (await context.ColorRoles.FirstOrDefaultAsync(
                    r => r.Id == e.RoleId,
                    Bot.StoppingToken
                ) is { } role)
                {
                    context.ColorRoles.Remove(role);
                    await context.SaveChangesAsync(Bot.StoppingToken);
                }
            }

            Logger.LogDebug("Role removed: {@ColorRole}", new { Id = e.RoleId, Guild = e.GuildId });
        }
    }
}