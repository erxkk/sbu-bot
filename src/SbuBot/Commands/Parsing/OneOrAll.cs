using System.Diagnostics.CodeAnalysis;

namespace SbuBot.Commands.Parsing
{
    // this is for non-generic pattern matching
    public interface IOneOrAll
    {
        [MemberNotNullWhen(true, nameof(IOneOrAll.Value))]
        public bool IsAll { get; }

        public object? Value { get; }
    }

    // never assign default value
    public sealed class OneOrAll<T> : IOneOrAll
    {
        [MemberNotNullWhen(true, nameof(OneOrAll<T>.Value))]
        public bool IsAll { get; }

        public T Value { get; }

        private OneOrAll() => IsAll = true;
        private OneOrAll(T value) => Value = value;

        public static OneOrAll<T> One(T value) => new(value);
        public static OneOrAll<T> All() => new();

        object? IOneOrAll.Value => Value;
    }
}