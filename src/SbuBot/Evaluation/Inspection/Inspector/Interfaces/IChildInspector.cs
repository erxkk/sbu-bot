namespace SbuBot.Evaluation.Inspection.Inspector
{
    public interface IChildInspector : IInspector
    {
        public IParentInspector Parent { get; }
    }
}