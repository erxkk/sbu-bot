using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ReminderService : DiscordBotService
    {
        private readonly ConcurrentDictionary<Snowflake, Entry> _scheduleEntries = new();

        public IReadOnlyDictionary<Snowflake, SbuReminder> GetReminders()
            => _scheduleEntries.ToDictionary(k => k.Key, v => v.Value.Reminder);

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach ((Snowflake _, Entry entry) in _scheduleEntries)
                await entry.Timer.DisposeAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<SbuReminder> reminders;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                reminders = await context.Reminders.ToListAsync(stoppingToken);
            }

            await Client.WaitUntilReadyAsync(stoppingToken);

            foreach (SbuReminder reminder in reminders)
            {
                if (reminder.DueAt + TimeSpan.FromSeconds(1) <= DateTimeOffset.Now)
                    reminder.DueAt = DateTimeOffset.Now + TimeSpan.FromSeconds(5);

                await ScheduleAsync(reminder, false);
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
            }

            _scheduleEntries[reminder.MessageId] = new(
                reminder,
                new(
                    _timerCallback,
                    reminder.MessageId,
                    reminder.DueAt - now,
                    Timeout.InfiniteTimeSpan
                )
            );

            Logger.LogDebug("Scheduled {@Reminder}", reminder);
        }

        public async Task RescheduleAsync(Snowflake id, DateTimeOffset newTimestamp)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            Entry entry;

            if (!_scheduleEntries.TryGetValue(id, out entry!))
            {
                Logger.LogWarning(
                    "Could not reschedule to {@NewTimestamp}, not found : {@Entry}",
                    newTimestamp,
                    id
                );

                return;
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
        }

        public async Task CancelAsync(Snowflake id)
        {
            if (!_scheduleEntries.TryRemove(id, out Entry? entry))
                return;

            await entry.Timer.DisposeAsync();
            SbuReminder reminder = entry.Reminder;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.Remove(reminder);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Cancelled: {@Reminder}", reminder);
        }

        public async Task CancelAsync(Func<SbuReminder, bool> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            int count = 0;

            IEnumerable<KeyValuePair<Snowflake, Entry>> filtered = _scheduleEntries.Where(e => query(e.Value.Reminder));
            IEnumerable<SbuReminder> reminders = filtered.Select(e => e.Value.Reminder);

            foreach ((Snowflake _, Entry entry) in filtered)
            {
                count++;
                await entry.Timer.DisposeAsync();
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.RemoveRange(reminders);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Cancelled: {@Reminders}", reminders);
        }

        private void _timerCallback(object? sender)
        {
            _ = _timerCallbackAsync((Snowflake)sender!);
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
                                    (reminder.Message ?? "No message given")
                                    + $"\n\n[Original Message]({reminder.JumpUrl})"
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

        private sealed class Entry
        {
            public SbuReminder Reminder { get; }
            public Timer Timer { get; }

            public Entry(SbuReminder reminder, Timer timer)
            {
                Reminder = reminder;
                Timer = timer;
            }
        }
    }
}