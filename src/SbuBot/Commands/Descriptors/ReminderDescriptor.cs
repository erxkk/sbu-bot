using System;

using SbuBot.Models;

namespace SbuBot.Commands.Descriptors
{
    public readonly struct ReminderDescriptor : IDescriptor
    {
        public static readonly string REMARKS = "This is a 2-part descriptor. The timestamp must be int he future, the "
            + $"message can be at most {SbuReminder.MAX_MESSAGE_LENGTH} characters long.";

        public DateTimeOffset Timestamp { get; init; }
        public string? Message { get; init; }
    }
}