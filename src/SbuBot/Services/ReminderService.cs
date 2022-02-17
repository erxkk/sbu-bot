using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using Kkommon;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Models;

namespace SbuBot.Services
{
    // TODO: use single delay to dispatch timers one by one
    // * fetch the next due reminder by letting the db order by timestamp
    // * on reschedule check if the reschedule would come before the currently running delay
    public sealed class ReminderService : DiscordBotService
    {
        private static readonly TimeSpan MAX_UNSUSPENDED_TIMESPAN = TimeSpan.FromDays(1);

        private record Entry(SbuReminder Reminder, Timer Timer);

        private readonly ConcurrentDictionary<Snowflake, Entry> _scheduleEntries = new();
        private readonly Timer _suspendedTimer;

        public ReminderService()
        {
            _suspendedTimer = new(
                _ => _ = _fetchSuspendedTimersAsync(),
                null,
                ReminderService.MAX_UNSUSPENDED_TIMESPAN,
                ReminderService.MAX_UNSUSPENDED_TIMESPAN
            );
        }

        public async Task<IReadOnlyDictionary<Snowflake, SbuReminder>> FetchRemindersAsync(
            Snowflake? userId,
            Snowflake? guildId
        )
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                IQueryable<SbuReminder> query = context.Reminders;

                if (userId is { })
                    query = query.Where(r => r.OwnerId == userId.Value);

                if (guildId is { })
                    query = query.Where(r => r.GuildId == guildId.Value);

                return await query.ToDictionaryAsync(r => r.MessageId, Bot.StoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _suspendedTimer.DisposeAsync();

            foreach ((Snowflake _, Entry entry) in _scheduleEntries)
                await entry.Timer.DisposeAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitUntilReadyAsync(stoppingToken);
            await _fetchSuspendedTimersAsync();
        }

        private async Task<IReadOnlyList<SbuReminder>> _fetchNextRemindersAsync()
        {
            DateTimeOffset maxDueAt = DateTimeOffset.Now + ReminderService.MAX_UNSUSPENDED_TIMESPAN;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                return await context.Reminders.Where(r => r.DueAt <= maxDueAt).ToListAsync(Bot.StoppingToken);
            }
        }

        public async Task ScheduleAsync(
            SbuReminder reminder,
            bool isNewReminder = true
        )
        {
            DateTimeOffset now = DateTimeOffset.Now;

            if (reminder.DueAt + TimeSpan.FromSeconds(1) <= now)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reminder),
                    reminder.DueAt,
                    "DueAt must not be less than 1s from now."
                );
            }

            if (isNewReminder)
            {
                using (IServiceScope scope = Bot.Services.CreateScope())
                {
                    SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                    context.Reminders.Add(reminder);
                    await context.SaveChangesAsync();
                }

                if (reminder.DueAt >= now + ReminderService.MAX_UNSUSPENDED_TIMESPAN)
                {
                    Logger.LogDebug("Scheduled Suspended {@Reminder}", reminder);
                    return;
                }
            }

            _scheduleEntries[reminder.MessageId] = new(
                reminder,
                new(
                    sender => _ = _dispatchReminderAsync((sender as SbuReminder)!),
                    reminder,
                    reminder.DueAt - now,
                    Timeout.InfiniteTimeSpan
                )
            );

            Logger.LogDebug("Scheduled {@Reminder}", reminder);
        }

        public async Task<bool> RescheduleAsync(Snowflake id, DateTimeOffset newTimestamp)
        {
            DateTimeOffset now = DateTimeOffset.Now;

            if (!_scheduleEntries.TryGetValue(id, out Entry? entry))
            {
                Logger.LogWarning(
                    "Could not reschedule to {@NewTimestamp}, not found : {@Entry}",
                    newTimestamp,
                    id
                );

                return false;
            }

            SbuReminder reminder = entry.Reminder;
            DateTimeOffset previousDueAt = reminder.DueAt;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                reminder.DueAt = newTimestamp;
                context.Reminders.Update(reminder);
                await context.SaveChangesAsync();
            }

            if (newTimestamp >= DateTimeOffset.Now + ReminderService.MAX_UNSUSPENDED_TIMESPAN)
                entry.Timer.Change(newTimestamp - now, Timeout.InfiniteTimeSpan);

            Logger.LogDebug(
                "Rescheduled: {@PreviousTimespan} -> {@NewTimespan} : {@Reminder}",
                previousDueAt,
                newTimestamp,
                reminder
            );

            return true;
        }

        public async Task<bool> CancelAsync(Snowflake id)
        {
            if (!_scheduleEntries.TryRemove(id, out Entry? entry))
                return false;

            await entry.Timer.DisposeAsync();
            SbuReminder reminder = entry.Reminder;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.Remove(reminder);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Cancelled: {@Reminder}", reminder);
            return true;
        }

        public async Task<IReadOnlyList<SbuReminder>> CancelAsync(Func<SbuReminder, bool> query)
        {
            List<SbuReminder> removed = new();

            IEnumerable<KeyValuePair<Snowflake, Entry>> filtered = _scheduleEntries.Where(e => query(e.Value.Reminder));
            IEnumerable<SbuReminder> reminders = filtered.Select(e => e.Value.Reminder);

            foreach ((Snowflake _, (SbuReminder reminder, Timer timer)) in filtered)
            {
                removed.Add(reminder);
                await timer.DisposeAsync();
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.RemoveRange(reminders);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Cancelled: {@Reminders}", reminders);
            return removed;
        }

        private async Task _dispatchReminderAsync(SbuReminder reminder)
        {
            await Bot.SendMessageAsync(
                reminder.ChannelId,
                new LocalMessage()
                    .WithReply(reminder.MessageId, reminder.ChannelId, reminder.GuildId)
                    .WithEmbeds(
                        new LocalEmbed()
                            .WithTitle("Reminder")
                            .WithDescription(
                                (reminder.Message ?? "`No Message`")
                                + $"\n\n{Markdown.Link("Original Message", reminder.GetJumpUrl())}"
                            )
                            .WithTimestamp(reminder.CreatedAt)
                    )
            );

            Logger.LogDebug("Dispatched {@Reminder}", reminder);

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.Remove(reminder);
                await context.SaveChangesAsync(Bot.StoppingToken);
            }
        }

        private async Task _fetchSuspendedTimersAsync()
        {
            IReadOnlyList<SbuReminder> reminders = await _fetchNextRemindersAsync();

            await Task.WhenAll(
                reminders.Select(
                    reminder =>
                    {
                        DateTimeOffset now = DateTimeOffset.Now;

                        if (reminder.DueAt + TimeSpan.FromSeconds(1) <= now)
                            reminder.DueAt = now + TimeSpan.FromSeconds(5);

                        return ScheduleAsync(reminder, false);
                    }
                )
            );
        }
    }
}
