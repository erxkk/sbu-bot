using System;
using System.Reflection;

namespace SbuBot.Inspection
{
    public static class Reflect
    {
        public static bool IsValueComparable(Type type)
            => type.IsAssignableTo(typeof(string)) || type.IsAssignableTo(typeof(ValueType));

        public static bool IsNumberType(Type type)
            => type == typeof(int)
                || type == typeof(ulong)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || type == typeof(uint)
                || type == typeof(ushort)
                || type == typeof(short)
                || type == typeof(byte)
                || type == typeof(sbyte);

        public static bool IsNullableNumberType(Type type)
            => type == typeof(int?)
                || type == typeof(ulong?)
                || type == typeof(long?)
                || type == typeof(float?)
                || type == typeof(double?)
                || type == typeof(decimal?)
                || type == typeof(uint?)
                || type == typeof(ushort?)
                || type == typeof(short?)
                || type == typeof(byte?)
                || type == typeof(sbyte?);

        public static object? SafeGetValue(this PropertyInfo @this, object obj)
        {
            object? value = null;
            bool isUndefined = false;

            try
            {
                value = @this.GetValue(obj);
            }
            catch
            {
                isUndefined = true;
            }

            return isUndefined ? "{undefExtract}" : value;
        }
    }
}