using System;
using System.Collections.Generic;
using System.Reflection;

using Disqord;

namespace SbuBot.Evaluation.Inspection
{
    public static partial class Inspect
    {
        private static readonly Dictionary<Type, Func<object, Extraction>> EXTRACTORS = new()
        {
            // simple built-in
            [typeof(object)] = Extraction.None,
            [typeof(string)] = @string => Extraction.Literal("\"" + @string + "\""),
            [typeof(bool)] = @bool => Extraction.Literal(@bool is true ? "true" : "false"),

            // rational
            [typeof(decimal)] = @decimal => Extraction.Literal(@decimal.ToString()),
            [typeof(double)] = @double => Extraction.Literal(@double.ToString()),
            [typeof(float)] = @float => Extraction.Literal(@float.ToString()),

            // signed integers
            [typeof(long)] = @long => Extraction.Literal(@long.ToString()),
            [typeof(int)] = @int => Extraction.Literal(@int.ToString()),
            [typeof(short)] = @short => Extraction.Literal(@short.ToString()),
            [typeof(sbyte)] = @sbyte => Extraction.Literal(@sbyte.ToString()),

            // unsigned integers
            [typeof(ulong)] = @ulong => Extraction.Literal(@ulong.ToString()),
            [typeof(uint)] = @uint => Extraction.Literal(@uint.ToString()),
            [typeof(ushort)] = @ushort => Extraction.Literal(@ushort.ToString()),
            [typeof(byte)] = @byte => Extraction.Literal(@byte.ToString()),

            // simple structs
            [typeof(Snowflake)] = snowflake => Extraction.Literal(snowflake.ToString()),
            [typeof(Guid)] = guid => Extraction.Literal(guid.ToString()),

            // wrappers
            [typeof(Optional<>)] = optional =>
            {
                Type type = optional.GetType();

                return type.GetProperty("HasValue")!.GetValue(optional) is true
                    ? Extraction.Extracted(type.GetProperty("Value")!.GetValue(optional))
                    : Extraction.Literal("none");
            },
        };

        public static IReadOnlyDictionary<Type, Func<object, Extraction>> Extractors => Inspect.EXTRACTORS;

        private static object? SafeGetProperty(PropertyInfo @this, object obj)
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

            return isUndefined
                ? obj.GetType().IsValueType
                    ? Activator.CreateInstance(obj.GetType())
                    : null
                : value;
        }

        public readonly struct Extraction
        {
            public readonly ExtractionType Type;
            public readonly object? Value;

            private Extraction(ExtractionType type, object? value)
            {
                Type = type;
                Value = value;
            }

            public static Extraction None(object? source)
                => new(ExtractionType.None, source);

            public static Extraction Literal(string? value)
                => new(ExtractionType.Literal, value);

            public static Extraction Extracted(object? value)
                => new(ExtractionType.Extracted, value);
        }

        public enum ExtractionType : byte
        {
            None, // not extracted
            Literal, // extracted to literal string representation
            Extracted, // extracted value
        }
    }
}