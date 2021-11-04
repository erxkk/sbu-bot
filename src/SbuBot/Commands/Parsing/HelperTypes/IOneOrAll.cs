using System.Diagnostics.CodeAnalysis;

namespace SbuBot.Commands.Parsing.HelperTypes
{
    public interface IOneOrAll
    {
        [MemberNotNullWhen(false, nameof(IOneOrAll.Value))]
        public bool IsAll { get; }

        public object? Value { get; }
    }
}