using System;
using System.Collections.Generic;

using SbuBot.Models;

namespace SbuBot.Commands.Parsing.Descriptors
{
    public readonly struct AutoResponseDescriptor : IDescriptor
    {
        public static readonly IReadOnlyDictionary<string, Type> PARTS = new Dictionary<string, Type>
        {
            ["trigger"] = typeof(string),
            ["response"] = typeof(string),
        };

        public static readonly string REMARKS = "This is a 2-part descriptor. The trigger or response must be at "
            + $"most {SbuAutoResponse.MAX_LENGTH} characters long.";

        public string Trigger { get; init; }
        public string Response { get; init; }
    }
}
