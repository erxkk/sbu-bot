using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Bot.Hosting;

using Microsoft.Extensions.Logging;

namespace SbuBot.Services
{
    public sealed class SchedulerService : SbuBotServiceBase
    {
        private readonly Dictionary<Guid, ScheduleEntry> _scheduleEntries = new();
        private readonly object _lock = new();

        public IReadOnlyDictionary<Guid, ScheduleEntry> ScheduleEntries
        {
            get
            {
                Dictionary<Guid, ScheduleEntry> copy;

                lock (_lock)
                {
                    copy = new(_scheduleEntries);
                }

                return copy;
            }
        }

        public SchedulerService(ILogger<SchedulerService> logger, DiscordBotBase bot) : base(logger, bot) { }

        public Guid Schedule(Func<ScheduleEntry, Task> callback, TimeSpan timeSpan, int recurringCount = 0)
        {
            var guid = Guid.NewGuid();
            Schedule(guid, callback, timeSpan, recurringCount);
            return guid;
        }

        public void Schedule(Guid id, Func<ScheduleEntry, Task> callback, TimeSpan timeSpan, int recurringCount = 0)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var timer = new Timer(
                timerCallback,
                id,
                timeSpan,
                recurringCount != 0 ? timeSpan : Timeout.InfiniteTimeSpan
            );

            lock (_lock)
            {
                _scheduleEntries[id] = new(callback, timer, recurringCount);
            }

            Logger.LogTrace("Scheduled {0} to {1}", id, timeSpan);

            void timerCallback(object? sender)
            {
                var identifier = (Guid) sender!;
                ScheduleEntry entry;

                lock (_lock)
                {
                    if (!_scheduleEntries.TryGetValue(identifier, out entry!))
                    {
                        Logger.LogWarning("Could not dispatch, not found : {0}", id);

                        return;
                    }

                    _ = entry.Callback(entry);

                    if (entry.RecurringCount == 0)
                    {
                        _scheduleEntries.Remove(identifier);
                        entry.Timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        entry.Timer.Dispose();
                    }
                    else if (entry.RecurringCount != -1)
                    {
                        _scheduleEntries[identifier] = entry with { RecurringCount = entry.RecurringCount - 1 };
                    }
                }

                Logger.LogTrace("Dispatched {0} with {@Entry}", id, entry);
            }
        }

        public void Reschedule(Guid id, TimeSpan timeSpan)
        {
            lock (_lock)
            {
                if (!_scheduleEntries.TryGetValue(id, out var entry))
                {
                    Logger.LogWarning("Could not reschedule to {0}, not found : {1}", timeSpan, id);

                    return;
                }

                entry.Timer.Change(timeSpan, timeSpan);
            }

            Logger.LogTrace("Rescheduled {0} to {1}", id, timeSpan);
        }

        public void Unschedule(Guid id)
        {
            lock (_lock)
            {
                if (!_scheduleEntries.Remove(id, out var entry))
                {
                    Logger.LogWarning("Could not unschedule, not found : {0}", id);

                    return;
                }

                entry.Timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                entry.Timer.Dispose();
            }

            Logger.LogTrace("Unscheduled {0}", id);
        }

        public sealed record ScheduleEntry(Func<ScheduleEntry, Task> Callback, Timer Timer, int RecurringCount);
    }
}