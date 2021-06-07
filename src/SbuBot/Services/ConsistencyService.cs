using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ConsistencyService : SbuBotServiceBase
    {
        public override int Priority => int.MaxValue;

        public ConsistencyService(ILogger<ConsistencyService> logger, DiscordBotBase bot) : base(logger, bot) { }

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            if (e.GuildId == SbuBotGlobals.Guild.ID)
                await Bot.Chunker.ChunkAsync(e.Guild, Bot.StoppingToken);

            await base.OnGuildAvailable(e);
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Members.Update(
                    await context.Members.FirstOrDefaultAsync(m => m.DiscordId == e.Member.Id, Bot.StoppingToken)
                    ?? new(e.Member.Id)
                );

                await context.SaveChangesAsync(Bot.StoppingToken);
            }

            await base.OnMemberJoined(e);
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            if (e.OldMember is null)
                return;

            if (e.OldMember.RoleIds.Count == e.NewMember.RoleIds.Count)
                return;

            IEnumerable<Snowflake> except = e.NewMember.RoleIds.Except(e.OldMember.RoleIds);

            if (except.Count() != 1)
                return;

            Snowflake addedRoleId = except.First();

            if (Bot.GetRole(e.NewMember.GuildId, addedRoleId) is not { } addedRole)
                throw new RequiredCacheException("Could not find required role in cache.");

            if (addedRole.Position >= Bot.ColorRoleSeparator.Position)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.ColorRoles.FirstOrDefaultAsync(r => r.DiscordId == addedRoleId) is { } role)
                {
                    role.OwnerId = e.MemberId;
                }
                else
                {
                    role = new(addedRole, e.MemberId);
                    context.ColorRoles.Add(role);
                }

                await context.SaveChangesAsync(Bot.StoppingToken);
            }

            Logger.LogInformation(
                "Color role assigned, established owner ship: {@ColorRole}",
                new { Id = addedRoleId, Owner = e.MemberId }
            );

            await base.OnMemberUpdated(e);
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                SchedulerService service = scope.ServiceProvider.GetRequiredService<SchedulerService>();

                if (await context.ColorRoles.FirstOrDefaultAsync(
                    m => m.OwnerId == e.User.Id,
                    Bot.StoppingToken
                ) is { } role)
                {
                    role.OwnerId = null;
                    context.ColorRoles.Update(role);
                }

                List<SbuTag> tags = await context.Tags
                    .Where(t => t.OwnerId == e.User.Id)
                    .ToListAsync(Bot.StoppingToken);

                List<SbuReminder> reminders = await context.Reminders
                    .Where(t => t.OwnerId == e.User.Id)
                    .Where(t => !t.IsDispatched)
                    .ToListAsync(Bot.StoppingToken);

                foreach (SbuTag tag in tags)
                {
                    tag.OwnerId = null;
                }

                foreach (SbuReminder reminder in reminders)
                {
                    service.Unschedule(reminder.Id);
                    reminder.IsDispatched = true;
                }

                context.Reminders.UpdateRange(reminders);
                context.Tags.UpdateRange(tags);

                await context.SaveChangesAsync(Bot.StoppingToken);
            }

            await base.OnMemberLeft(e);
        }

        protected override async ValueTask OnRoleDeleted(RoleDeletedEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.ColorRoles.FirstOrDefaultAsync(
                    r => r.DiscordId == e.RoleId,
                    Bot.StoppingToken
                ) is { } role)
                {
                    context.ColorRoles.Remove(role);
                    await context.SaveChangesAsync(Bot.StoppingToken);
                }
            }

            await base.OnRoleDeleted(e);
        }
    }
}