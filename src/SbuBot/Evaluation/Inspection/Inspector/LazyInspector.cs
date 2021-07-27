using System;

using Kkommon;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public abstract class LazyInspector : Inspector, ILazyInspector
    {
        public Lazy<IEagerInspector> Inspector { get; }

        protected LazyInspector(Func<object> valueFactory, Type type)
            : base(type)
        {
            Preconditions.NotNull(valueFactory, nameof(valueFactory));

            Inspector = new(() => IInspector.FromUnknown(valueFactory()));
        }
    }
}