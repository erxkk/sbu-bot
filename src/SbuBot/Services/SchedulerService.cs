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

        public Guid Schedule(
            Func<Entry, Task> callback,
            TimeSpan timeSpan,
            int recurringCount = 0,
            CancellationToken cancellationToken = default
        )
        {
            var guid = Guid.NewGuid();
            Schedule(guid, callback, timeSpan, recurringCount, cancellationToken);
            return guid;
        }

        public void Schedule(
            Guid id,
            Func<Entry, Task> callback,
            TimeSpan timeSpan,
            int recurringCount = 0,
            CancellationToken cancellationToken = default
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

            Entry newEntry;

            lock (_lock)
            {
                _linkIfNotStoppingToken(ref cancellationToken);
                cancellationToken.Register(() => Cancel(id));
                _scheduleEntries[id] = newEntry = new(id, callback, timer, recurringCount, cancellationToken);
            }

            Logger.LogTrace("Scheduled: {@Entry} ", newEntry);

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

                Logger.LogTrace("Dispatched: {@Entry}", entry);
            }
        }

        public void Reschedule(Guid id, TimeSpan timeSpan, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (!_scheduleEntries.TryGetValue(id, out var entry))
                {
                    Logger.LogWarning("Could not reschedule to {@NewTimespan}, not found : {@Entry}", timeSpan, id);

                    return;
                }

                _linkIfNotStoppingToken(ref cancellationToken);
                cancellationToken.Register(() => Cancel(id));
                entry.Timer.Change(timeSpan, timeSpan);
                _scheduleEntries[id] = entry with { CancellationToken = cancellationToken };
            }

            Logger.LogTrace("Rescheduled: {@Entry} -> {@NewTimespan}", id, timeSpan);
        }

        public void Cancel(Guid id)
        {
            lock (_lock)
            {
                if (!_scheduleEntries.Remove(id, out var entry))
                {
                    Logger.LogWarning("Could not cancel, not found : {@Entry}", id);

                    return;
                }

                entry.Timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                entry.Timer.Dispose();
            }

            Logger.LogTrace("Unscheduled: {@Entry}", id);
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
            Guid Id,
            Func<Entry, Task> Callback,
            Timer Timer,
            int RecurringCount,
            CancellationToken CancellationToken = default
        );
    }
}