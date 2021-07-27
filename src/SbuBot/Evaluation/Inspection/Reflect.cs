using System;
using System.Reflection;

namespace SbuBot.Evaluation.Inspection
{
    public static class Reflect
    {
        public static bool IsValueComparable(Type type)
            => type.IsAssignableTo(typeof(ValueType)) || type.IsAssignableTo(typeof(string));

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

        public static bool IsGenericType(Type type, Type genericTypeDefinition)
            => type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
    }
}