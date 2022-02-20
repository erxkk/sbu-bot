using System;
using System.Collections.Generic;

using SbuBot.Models;

namespace SbuBot.Commands.Parsing.Descriptors
{
    public readonly struct TagDescriptor : IDescriptor
    {
        public static readonly IReadOnlyDictionary<string, Type> PARTS = new Dictionary<string, Type>
        {
            ["name"] = typeof(string),
            ["content"] = typeof(string),
        };

        public static readonly string REMARKS = "This is a 2-part descriptor. The name must be at least "
            + $"{SbuTag.MIN_NAME_LENGTH} and at most {SbuTag.MAX_NAME_LENGTH} characters long. The content must be at "
            + $"most {SbuTag.MAX_CONTENT_LENGTH} characters long.";

        public string Name { get; init; }
        public string Content { get; init; }
    }
}
