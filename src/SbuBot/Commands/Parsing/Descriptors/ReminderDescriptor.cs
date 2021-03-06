using System;
using System.Collections.Generic;

using SbuBot.Models;

namespace SbuBot.Commands.Parsing.Descriptors
{
    public readonly struct ReminderDescriptor : IDescriptor
    {
        public static readonly IReadOnlyDictionary<string, Type> PARTS = new Dictionary<string, Type>
        {
            ["timestamp"] = typeof(DateTime),
            ["message"] = typeof(string),
        };

        public static readonly string REMARKS = "This is a 2-part descriptor. The timestamp must be int he future, the "
            + $"message can be at most {SbuReminder.MAX_MESSAGE_LENGTH} characters long.";

        public DateTime Timestamp { get; init; }
        public string Message { get; init; }
    }
}
