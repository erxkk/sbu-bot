using System;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public abstract class Inspector : IInspector
    {
        public Type Type { get; }

        protected Inspector(Type type) => Type = type;
    }
}