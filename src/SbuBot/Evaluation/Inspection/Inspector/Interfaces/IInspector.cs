using System;
using System.Collections;

namespace SbuBot.Evaluation.Inspection.Inspector
{
    public interface IInspector
    {
        public Type Type { get; }

        public static IEagerInspector FromUnknown(object obj)
        {
            if (obj is IEnumerable enumerable)
                return new EnumerableInspector(enumerable);

            if (IsPrimitive(obj))
                return new PrimitiveInspector(obj);

            return new ObjectInspector(obj);
        }

        public static bool IsPrimitive(object obj)
        {
            // TODO: allow optionals
            Type type = obj.GetType();
            return obj is string || Reflect.IsNumberType(type) || Reflect.IsNullableNumberType(type);
        }
    }
}