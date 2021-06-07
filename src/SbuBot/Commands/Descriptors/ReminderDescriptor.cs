using System;

namespace SbuBot.Commands.Descriptors
{
    public readonly struct ReminderDescriptor : IDescriptor
    {
        public DateTimeOffset Timestamp { get; init; }
        public string? Message { get; init; }
    }
}