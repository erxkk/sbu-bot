using System;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public interface ILazyInspector : IInspector
    {
        public Lazy<IEagerInspector> Inspector { get; }
    }
}