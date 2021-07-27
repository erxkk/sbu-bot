using System.Collections.Generic;

using Kkommon;
using Kkommon.Extensions.String;

using SbuBot.Extensions;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public abstract class EagerInspector : Inspector, IEagerInspector
    {
        public object Value { get; }

        protected EagerInspector(object value) : base(value?.GetType()!)
        {
            Preconditions.NotNull(value, nameof(value));

            Value = value!;
        }

        public IEnumerable<string> YieldInspections()
        {
            string inspection = Value.GetInspection();
            return inspection.Length > 4096 ? inspection.Chunk(2048) : new[] { inspection };
        }
    }
}