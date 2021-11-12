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
    public sealed class ReminderService : DiscordBotService
    {
        private static readonly TimeSpan MAX_UNSUSPENDED_TIMESPAN = TimeSpan.FromDays(7);

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

        public IReadOnlyDictionary<Snowflake, SbuReminder> GetReminders()
            => _scheduleEntries.ToDictionary(k => k.Key, v => v.Value.Reminder);

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

        private async Task<IReadOnlyList<SbuReminder>> FetchNextRemindersAsync()
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

                if (now + ReminderService.MAX_UNSUSPENDED_TIMESPAN <= reminder.DueAt)
                {
                    Logger.LogDebug("Scheduled Suspended {@Reminder}", reminder);
                    return;
                }
            }

            _scheduleEntries[reminder.MessageId] = new(
                reminder,
                new(
                    sender => _ = _timerCallbackAsync((Snowflake)sender!),
                    reminder.MessageId,
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
            Preconditions.NotNull(query, nameof(query));

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

        private async Task _timerCallbackAsync(Snowflake identifier)
        {
            if (!_scheduleEntries.TryRemove(identifier, out Entry? entry))
            {
                Logger.LogWarning("Could not dispatch, not found : {@Entry}", identifier);
                return;
            }

            await entry.Timer.DisposeAsync();
            SbuReminder reminder = entry.Reminder;

            try
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
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception while dispatching {@Reminder}", reminder);
            }
            finally
            {
                using (IServiceScope scope = Bot.Services.CreateScope())
                {
                    SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                    context.Reminders.Remove(reminder);
                    await context.SaveChangesAsync();
                }

                Logger.LogTrace("Removed internally {@Reminder}", reminder);
            }
        }

        private async Task _fetchSuspendedTimersAsync()
        {
            IReadOnlyList<SbuReminder> reminders = await FetchNextRemindersAsync();

            await Task.WhenAll(
                reminders.Select(
                    reminder =>
                    {
                        if (reminder.DueAt + TimeSpan.FromSeconds(1) <= DateTimeOffset.Now)
                            reminder.DueAt = DateTimeOffset.Now + TimeSpan.FromSeconds(5);

                        return ScheduleAsync(reminder, false);
                    }
                )
            );
        }
    }
}
