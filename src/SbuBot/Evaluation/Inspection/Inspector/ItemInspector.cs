using Kkommon;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public sealed class ItemInspector : EagerInspector, IChildInspector
    {
        public IParentInspector Parent { get; }

        public ItemInspector(object value, EnumerableInspector parent) : base(value)
        {
            Preconditions.NotNull(parent, nameof(parent));

            Parent = parent;
        }
    }
}