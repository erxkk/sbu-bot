using System.Collections.Generic;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public interface IParentInspector : IInspector
    {
        public IEnumerable<IChildInspector> YieldInspectors();
    }
}