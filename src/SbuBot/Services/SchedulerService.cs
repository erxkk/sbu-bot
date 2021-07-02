using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Kkommon.Extensions.Arithmetic;

using Microsoft.Extensions.Logging;

namespace SbuBot.Services
{
    public sealed class SchedulerService : SbuBotServiceBase
    {
        private readonly Dictionary<Guid, Entry> _scheduleEntries = new();

        public SchedulerService(SbuConfiguration configuration) : base(configuration) { }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                foreach (KeyValuePair<Guid, Entry> keyValuePair in _scheduleEntries)
                    keyValuePair.Value.Timer.Dispose();
            }

            return Task.CompletedTask;
        }

        public Guid Schedule(
            Func<Entry, Task> callback,
            TimeSpan timeSpan,
            int recurringCount = 0
        )
        {
            var guid = Guid.NewGuid();
            Schedule(guid, callback, timeSpan, recurringCount);
            return guid;
        }

        public void Schedule(
            Guid id,
            Func<Entry, Task> callback,
            TimeSpan timeSpan,
            int recurringCount = 0
        )
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            if (timeSpan.IsLessThan(TimeSpan.FromSeconds(1)))
                throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan, "Timespan cannot be less than 1s.");

            var timer = new Timer(
                _timerCallback,
                id,
                timeSpan,
                recurringCount != 0 ? timeSpan : Timeout.InfiniteTimeSpan
            );

            Entry newEntry;

            lock (this)
            {
                _scheduleEntries[id] = newEntry = new(id, callback, timer, recurringCount);
            }

            Logger.LogTrace("Scheduled: {@Entry} ", newEntry);
        }

        private void _timerCallback(object? sender)
        {
            var identifier = (Guid) sender!;
            Entry entry;

            lock (this)
            {
                if (!_scheduleEntries.TryGetValue(identifier, out entry!))
                {
                    Logger.LogWarning("Could not dispatch, not found : {@Entry}", identifier);
                    return;
                }

                // discarded task
                _ = entry.Callback(entry);

                if (entry.RecurringCount == 0)
                {
                    _scheduleEntries.Remove(identifier);
                    entry.Timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    entry.Timer.Dispose();
                }
                else if (entry.RecurringCount != -1)
                {
                    entry.RecurringCount--;
                }
            }

            Logger.LogTrace("Dispatched: {@Entry}", entry);
        }

        public bool Reschedule(Guid id, TimeSpan timeSpan)
        {
            Entry? entry;

            lock (this)
            {
                if (!_scheduleEntries.TryGetValue(id, out entry))
                {
                    Logger.LogWarning("Could not reschedule to {@NewTimespan}, not found : {@Entry}", timeSpan, id);
                    return false;
                }

                entry.Timer.Change(timeSpan, timeSpan);
            }

            Logger.LogTrace("Rescheduled: {@Entry} -> {@NewTimespan}", entry, timeSpan);

            return true;
        }

        public bool Cancel(Guid id)
        {
            Entry? entry;

            lock (this)
            {
                if (!_scheduleEntries.Remove(id, out entry))
                {
                    Logger.LogWarning("Could not cancel, not found : {@Entry}", id);
                    return false;
                }

                entry.Timer.Dispose();
            }

            Logger.LogTrace("Cancelled: {@Entry}", entry);
            return true;
        }

        public sealed class Entry
        {
            public Guid Id { get; }
            public Func<Entry, Task> Callback { get; }
            public Timer Timer { get; }
            public int RecurringCount { get; set; }
            public CancellationToken CancellationToken { get; }

            public Entry(
                Guid id,
                Func<Entry, Task> callback,
                Timer timer,
                int recurringCount,
                CancellationToken cancellationToken = default
            )
            {
                Id = id;
                Callback = callback;
                Timer = timer;
                RecurringCount = recurringCount;
                CancellationToken = cancellationToken;
            }
        }
    }
}