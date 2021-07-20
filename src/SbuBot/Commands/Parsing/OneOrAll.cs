namespace SbuBot.Commands.Parsing
{
    // this is for non-generic pattern matching
    internal interface IOneOrAll
    {
        public interface ISpecific : IOneOrAll
        {
            public object? Value { get; }
        }

        public interface IAll : IOneOrAll { }
    }

    // never assign default value
    public abstract class OneOrAll<TSpecific> : IOneOrAll
    {
        private OneOrAll() { }

        public sealed class Specific : OneOrAll<TSpecific>, IOneOrAll.ISpecific
        {
            public TSpecific Value { get; }
            public Specific(TSpecific value) => Value = value;

            object? IOneOrAll.ISpecific.Value => Value;
        }

        public sealed class All : OneOrAll<TSpecific>, IOneOrAll.IAll { }
    }
}