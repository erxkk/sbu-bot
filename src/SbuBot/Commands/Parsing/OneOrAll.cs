using System.Diagnostics.CodeAnalysis;

namespace SbuBot.Commands.Parsing
{
    // this is for non-generic pattern matching
    internal interface IOneOrAll
    {
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsAll { get; }

        public object? Value { get; }
    }

    // never assign default value
    public sealed class OneOrAll<T> : IOneOrAll
    {
        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsAll { get; }

        public T Value { get; }

        private OneOrAll() => IsAll = true;
        private OneOrAll(T value) => Value = value;

        public static OneOrAll<T> One(T value) => new(value);
        public static OneOrAll<T> All() => new();

        object? IOneOrAll.Value => Value;
    }
}