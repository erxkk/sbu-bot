using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.Extensions.Logging;

namespace SbuBot.Services
{
    public sealed class SchedulerService : SbuBotServiceBase
    {
        private readonly Dictionary<Guid, Entry> _scheduleEntries = new();
        private readonly object _lock = new();

        public IReadOnlyDictionary<Guid, Entry> ScheduleEntries
        {
            get
            {
                Dictionary<Guid, Entry> copy;

                lock (_lock)
                {
                    copy = new(_scheduleEntries);
                }

                return copy;
            }
        }

        public SchedulerService(ILogger<SchedulerService> logger, DiscordBotBase bot) : base(logger, bot) { }

        public Guid Schedule(Func<Entry, Task> callback, TimeSpan timeSpan, int recurringCount = 0)
        {
            var guid = Guid.NewGuid();
            Schedule(guid, callback, timeSpan, recurringCount);
            return guid;
        }

        public void Schedule(
            Guid id,
            Func<Entry, Task> callback,
            TimeSpan timeSpan,
            int recurringCount = 0,
            CancellationToken unschedulingToken = default
        )
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
                _linkIfNotStoppingToken(ref unschedulingToken);
                unschedulingToken.Register(() => Unschedule(id));
                _scheduleEntries[id] = new(callback, timer, recurringCount, unschedulingToken);
            }

            Logger.LogTrace("Scheduled {@Entry} to {@Timespan}", id, timeSpan);

            void timerCallback(object? sender)
            {
                var identifier = (Guid) sender!;
                Entry entry;

                lock (_lock)
                {
                    if (!_scheduleEntries.TryGetValue(identifier, out entry!))
                    {
                        Logger.LogWarning("Could not dispatch, not found : {@Entry}", id);

                        return;
                    }

                    // unawaited task
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

        public void Reschedule(Guid id, TimeSpan timeSpan, CancellationToken unschedulingToken = default)
        {
            lock (_lock)
            {
                if (!_scheduleEntries.TryGetValue(id, out var entry))
                {
                    Logger.LogWarning("Could not reschedule to {@Timespan}, not found : {@Entry}", timeSpan, id);

                    return;
                }

                _linkIfNotStoppingToken(ref unschedulingToken);
                unschedulingToken.Register(() => Unschedule(id));
                entry.Timer.Change(timeSpan, timeSpan);
                _scheduleEntries[id] = entry with { CancellationToken = unschedulingToken };
            }

            Logger.LogTrace("Rescheduled {0} to {1}", id, timeSpan);
        }

        public void Unschedule(Guid id)
        {
            lock (_lock)
            {
                if (!_scheduleEntries.Remove(id, out var entry))
                {
                    Logger.LogWarning("Could not unschedule, not found : {@Entry}", id);

                    return;
                }

                entry.Timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                entry.Timer.Dispose();
            }

            Logger.LogTrace("Unscheduled {0}", id);
        }

        private void _linkIfNotStoppingToken(ref CancellationToken cancellationToken)
        {
            if (cancellationToken != Bot.StoppingToken)
            {
                cancellationToken = CancellationTokenSource
                    .CreateLinkedTokenSource(Bot.StoppingToken, cancellationToken)
                    .Token;
            }
        }

        public sealed record Entry(
            Func<Entry, Task> Callback,
            Timer Timer,
            int RecurringCount,
            CancellationToken CancellationToken = default
        );
    }
}