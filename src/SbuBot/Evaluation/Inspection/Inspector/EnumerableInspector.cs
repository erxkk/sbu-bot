using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Kkommon;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public sealed class EnumerableInspector : EagerInspector, IParentInspector
    {
        public new IEnumerable Value { get; }
        public Type ItemType { get; }

        public EnumerableInspector(IEnumerable value) : base(value.GetType())
        {
            Preconditions.NotNull(value, nameof(value));

            Type type = value.GetType();

            Value = value;

            ItemType = type.IsGenericType
                ? type.GetInterfaces()
                    .First(i => i.GetGenericArguments().Length == 1 && i.Name.StartsWith("IEnumerable"))
                    .GetGenericArguments()[0]
                : typeof(object);
        }

        public IEnumerable<IChildInspector> YieldInspectors()
        {
            foreach (object obj in Value)
                yield return new ItemInspector(obj, this);
        }
    }
}