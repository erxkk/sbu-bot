using System;

using Kkommon;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public abstract class MemberInspector : LazyInspector, IChildInspector
    {
        public IParentInspector Parent { get; }
        public string Name { get; }

        protected MemberInspector(string name, Func<object> valueFactory, ObjectInspector parent, Type type)
            : base(valueFactory, type)
        {
            Preconditions.NotNull(name, nameof(name));
            Preconditions.NotNull(parent, nameof(parent));

            Name = name;
            Parent = parent;
        }
    }
}