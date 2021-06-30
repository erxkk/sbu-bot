using System;
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
        private readonly SchedulerService _schedulerService;

        private readonly Dictionary<Guid, SbuReminder> _currentReminders = new();

        public ReminderService(SchedulerService schedulerService) => _schedulerService = schedulerService;

        public IReadOnlyDictionary<Guid, SbuReminder> GetCurrentReminders()
        {
            Dictionary<Guid, SbuReminder> copy;

            lock (this)
            {
                copy = new(_currentReminders);
            }

            return copy;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<SbuReminder> notDispatchedReminders;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                notDispatchedReminders = await context.Reminders.ToListAsync(stoppingToken);
            }

            await Client.WaitUntilReadyAsync(stoppingToken);

            foreach (SbuReminder notDispatchedReminder in notDispatchedReminders)
            {
                if (notDispatchedReminder.DueAt + TimeSpan.FromMilliseconds(500) <= DateTimeOffset.Now)
                    notDispatchedReminder.DueAt = DateTimeOffset.Now + TimeSpan.FromSeconds(5);

                await ScheduleAsync(notDispatchedReminder, false);
            }

            await base.ExecuteAsync(stoppingToken);
        }

        public async Task ScheduleAsync(
            SbuReminder reminder,
            bool isNewReminder = true
        )
        {
            if (reminder.DueAt + TimeSpan.FromMilliseconds(500) <= DateTimeOffset.Now)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reminder),
                    reminder.DueAt,
                    "DueAt must not be less than now."
                );
            }

            if (isNewReminder)
            {
                using (IServiceScope scope = Bot.Services.CreateScope())
                {
                    SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                    context.Add(reminder);
                    await context.SaveChangesAsync();
                }
            }

            lock (this)
            {
                _currentReminders[reminder.Id] = reminder;
            }

            _schedulerService.Schedule(
                reminder.Id,
                reminderCallback,
                reminder.DueAt - DateTimeOffset.Now,
                0
            );

            Logger.LogDebug("Scheduled {@Reminder}", reminder.Id);

            async Task reminderCallback(SchedulerService.Entry entry)
            {
                try
                {
                    await Bot.SendMessageAsync(
                        reminder.ChannelId,
                        new LocalMessage()
                            .WithReply(reminder.MessageId, reminder.ChannelId, SbuGlobals.Guild.SELF, false)
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

                    Logger.LogDebug("Dispatched {@Reminder}", reminder.Id);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception while dispatching {@Reminder}", reminder);
                }
                finally
                {
                    lock (this)
                    {
                        _currentReminders.Remove(reminder.Id);
                    }

                    using (IServiceScope scope = Bot.Services.CreateScope())
                    {
                        SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                        context.Reminders.Remove(reminder);
                        await context.SaveChangesAsync(entry.CancellationToken);
                    }

                    Logger.LogTrace("Removed internally {@Reminder}", reminder.Id);
                }
            }
        }

        public async Task RescheduleAsync(Guid id, DateTimeOffset newTimestamp)
        {
            SbuReminder reminder;

            lock (this)
            {
                if (!_currentReminders.TryGetValue(id, out reminder!))
                    return;
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                reminder.DueAt = newTimestamp;
                context.Reminders.Update(reminder);
                await context.SaveChangesAsync();
            }

            DateTimeOffset previousDueAt = reminder.DueAt;
            reminder.DueAt = newTimestamp;
            _schedulerService.Reschedule(id, reminder.DueAt - DateTimeOffset.Now);

            Logger.LogDebug(
                "Rescheduled: {@PreviousTimespan} -> {@NewTimespan} : {@Reminder}",
                previousDueAt,
                reminder.DueAt,
                reminder.Id
            );
        }

        public async Task CancelAsync(Guid id)
        {
            SbuReminder reminder;

            lock (this)
            {
                if (!_currentReminders.Remove(id, out reminder!))
                    return;
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.Remove(reminder);
                await context.SaveChangesAsync();
            }

            _schedulerService.Cancel(id);

            Logger.LogDebug("Unscheduled: {@Reminder}", reminder.Id);
        }

        public async Task CancelAsync(Func<KeyValuePair<Guid, SbuReminder>, bool> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            int count = 0;

            IEnumerable<KeyValuePair<Guid, SbuReminder>> reminders = GetCurrentReminders().Where(query);

            lock (this)
            {
                foreach ((Guid id, SbuReminder _) in reminders)
                {
                    count++;
                    _currentReminders.Remove(id);
                    _schedulerService.Cancel(id);
                }
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.RemoveRange(reminders.Select(r => r.Value));
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Unscheduled: {@Reminders}", new { Amount = count });
        }
    }
}