using System.Reflection;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public sealed class FieldInspector : MemberInspector
    {
        public FieldInfo FieldInfo { get; }

        public FieldInspector(FieldInfo fieldInfo, ObjectInspector parent)
            : base(fieldInfo.Name, () => fieldInfo.GetValue(parent)!, parent, fieldInfo.FieldType)
            => FieldInfo = fieldInfo;
    }
}