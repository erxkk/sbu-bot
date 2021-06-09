using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ReminderService : SbuBotServiceBase
    {
        private readonly SchedulerService _schedulerService;

        private readonly Dictionary<Guid, SbuReminder> _currentReminders = new();
        private readonly object _lock = new();

        public IReadOnlyDictionary<Guid, SbuReminder> CurrentReminders
        {
            get
            {
                Dictionary<Guid, SbuReminder> copy;

                lock (_lock)
                {
                    copy = new(_currentReminders);
                }

                return copy;
            }
        }

        public ReminderService(
            SchedulerService schedulerService,
            ILogger<ReminderService> logger,
            DiscordBotBase bot
        ) : base(logger, bot)
            => _schedulerService = schedulerService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<SbuReminder> notDispatchedReminders;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                notDispatchedReminders = await context.Reminders
                    .Where(r => !r.IsDispatched)
                    .ToListAsync(stoppingToken);
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

        public async Task ScheduleAsync(SbuReminder reminder, bool isNewReminder = true)
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

            lock (_lock)
            {
                _currentReminders[reminder.Id] = reminder;
            }

            _schedulerService.Schedule(reminder.Id, reminderCallback, reminder.DueAt - DateTimeOffset.Now);

            Logger.LogDebug("Scheduled {@Reminder}", reminder.Id);

            async Task reminderCallback(SchedulerService.ScheduleEntry entry)
            {
                try
                {
                    await Bot.SendMessageAsync(
                        reminder.ChannelId,
                        new LocalMessage()
                            .WithReply(reminder.MessageId, reminder.ChannelId, SbuGlobals.Guild.SELF, false)
                            .WithEmbed(
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
                    lock (_lock)
                    {
                        _currentReminders.Remove(reminder.Id);
                    }

                    using (IServiceScope scope = Bot.Services.CreateScope())
                    {
                        SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                        reminder.IsDispatched = true;
                        context.Reminders.Update(reminder);
                        await context.SaveChangesAsync();
                    }

                    Logger.LogTrace("Removed internally {@Reminder}", reminder.Id);
                }
            }
        }

        public async Task RescheduleAsync(Guid id, DateTimeOffset newTimestamp)
        {
            SbuReminder reminder;

            lock (_lock)
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
                "Rescheduled from {0} to {1} : {@Reminder}",
                previousDueAt,
                reminder.DueAt,
                reminder.Id
            );
        }

        public async Task UnscheduleAsync(Guid id)
        {
            SbuReminder reminder;

            lock (_lock)
            {
                if (!_currentReminders.Remove(id, out reminder!))
                    return;
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                reminder.IsDispatched = true;
                context.Reminders.Update(reminder);
                await context.SaveChangesAsync();
            }

            _schedulerService.Unschedule(id);

            Logger.LogDebug("Unscheduled {@Reminder}", reminder.Id);
        }

        public async Task UnscheduleAsync(Snowflake ownerId)
        {
            int count = 0;

            IEnumerable<KeyValuePair<Guid, SbuReminder>> reminders = CurrentReminders
                .Where(r => r.Value.OwnerId == ownerId);

            lock (_lock)
            {
                foreach ((Guid id, SbuReminder reminder) in reminders)
                {
                    count++;
                    reminder.IsDispatched = true;
                    _currentReminders.Remove(id);
                    _schedulerService.Unschedule(id);
                }
            }

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                context.Reminders.UpdateRange(reminders.Select(r => r.Value));
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Unscheduled {@Count} reminders for {@Owner}", count, ownerId);
        }
    }
}