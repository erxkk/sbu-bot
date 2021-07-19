using SbuBot.Models;

namespace SbuBot.Commands.Parsing.Descriptors
{
    public readonly struct AutoResponseDescriptor : IDescriptor
    {
        public static readonly string REMARKS = "This is a 2-part descriptor. The trigger or response must be at "
            + $"most {SbuAutoResponse.MAX_LENGTH} characters long.";

        public string Trigger { get; init; }
        public string Response { get; init; }
    }
}