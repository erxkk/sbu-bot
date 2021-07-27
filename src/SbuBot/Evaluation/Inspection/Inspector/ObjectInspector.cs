using System.Collections.Generic;
using System.Reflection;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public class ObjectInspector : EagerInspector, IParentInspector
    {
        public ObjectInspector(object value) : base(value) { }

        public IEnumerable<IChildInspector> YieldInspectors()
        {
            foreach (FieldInfo fieldInfo in Type.GetFields())
                yield return new FieldInspector(fieldInfo, this);

            foreach (PropertyInfo propertyInfo in Type.GetProperties())
                yield return new PropertyInspector(propertyInfo, this);
        }
    }
}