using System.Collections.Generic;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public interface IEagerInspector : IInspector
    {
        public IEnumerable<string> YieldInspections();
    }
}