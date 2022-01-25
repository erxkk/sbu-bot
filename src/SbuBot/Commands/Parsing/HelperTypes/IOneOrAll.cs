using System.Diagnostics.CodeAnalysis;

namespace SbuBot.Commands.Parsing.HelperTypes
{
    // this is for non-generic pattern matching
    public interface IOneOrAll
    {
        [MemberNotNullWhen(false, nameof(IOneOrAll.Value))]
        public bool IsAll { get; }

        public object? Value { get; }
    }
}
