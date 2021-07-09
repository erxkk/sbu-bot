namespace SbuBot.Commands.Parsing
{
    // never assign default value
    public abstract class OneOrAll<TSpecific>
    {
        private OneOrAll() { }

        public sealed class Specific : OneOrAll<TSpecific>
        {
            public TSpecific Value { get; }
            public Specific(TSpecific value) => Value = value;
        }

        public sealed class All : OneOrAll<TSpecific> { }
    }
}