using System;
using System.Reflection;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public sealed class PropertyInspector : MemberInspector
    {
        public PropertyInfo PropertyInfo { get; }

        public PropertyInspector(PropertyInfo propertyInfo, ObjectInspector parent)
            : base(propertyInfo.Name, () => propertyInfo.GetValue(parent)!, parent, propertyInfo.PropertyType)
        {
            if (!propertyInfo.CanRead)
                throw new ArgumentException("The property is write only.", nameof(propertyInfo));

            PropertyInfo = propertyInfo;
        }
    }
}