using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Disqord;

using ExtractorResult = Kkommon.Result<object?, string>;

namespace SbuBot.Inspection
{
    // TODO: cleanup, stream line extraction process
    public static class Inspect
    {
        public const int MAX_EMBED_CODE_BLOCK_WIDTH = 60;

        public static readonly Dictionary<Type, Func<object, string>> LITERALS = new()
        {
            [typeof(Snowflake)] = snowflake => ((Snowflake) snowflake).RawValue.ToString(),
            [typeof(Guid)] = guid => guid.ToString()!,
            [typeof(bool)] = @bool => @bool is true ? "true" : "false",
        };

        public static readonly Dictionary<Type, Func<object, Type, ExtractorResult>> EXTRACTORS = new()
        {
            [typeof(Optional<>)] = (optional, nonGenericType)
                => nonGenericType.GetProperty("HasValue")!.GetValue(optional) is true
                    ? new ExtractorResult.Success(nonGenericType.GetProperty("Value")!.GetValue(optional))
                    : new ExtractorResult.Error("none"),
        };

        public static void AppendInspectionTo(
            StringBuilder builder,
            object obj,
            int depth,
            int indentationDelta = 2,
            int itemCount = 5,
            int maxStringLength = 32
        ) => AppendObjectInspectionTo(
            builder,
            obj,
            new HashSet<object>(),
            depth,
            0,
            indentationDelta,
            itemCount,
            maxStringLength
        );

        private static void AppendObjectInspectionTo(
            StringBuilder builder,
            object? obj,
            ISet<object> traversedObjects,
            int depth,
            int indentation,
            int indentationDelta,
            int itemCount,
            int maxStringLength
        )
        {
            Type type = obj?.GetType()!;

            // TODO: add enum literal entry
            // BUG: incorrect optional extraction
            if (obj is { }
                && type.IsGenericType
                && EXTRACTORS.TryGetValue(type.GetGenericTypeDefinition(), out var extractor))
            {
                switch (extractor(obj, type))
                {
                    case ExtractorResult.Success success:
                        obj = success.Value;
                        break;

                    case ExtractorResult.Error error:
                        builder.Append(error.Value);
                        break;

                    // unreachable
                    default:
                        throw new();
                }
            }

            if (obj is null)
            {
                builder.Append("null");
                return;
            }

            if (!Reflect.IsValueComparable(type) && !traversedObjects.Add(obj))
            {
                builder.Append("{@}");
                return;
            }

            if (LITERALS.GetValueOrDefault(type) is { } literalizer)
            {
                builder.Append(literalizer(obj));
                return;
            }

            if (obj is string str)
            {
                builder.Append('"');

                if (str.Length > maxStringLength)
                {
                    builder.Append(str[..maxStringLength])
                        .Append('"')
                        .Append(SbuGlobals.ELLIPSES);
                }
                else
                {
                    builder.Append(str).Append('"');
                }

                return;
            }

            if (obj is IEnumerable enumerable)
            {
                AppendEnumerableInspectionTo(
                    builder,
                    enumerable,
                    traversedObjects,
                    depth,
                    indentation,
                    indentationDelta,
                    itemCount,
                    maxStringLength
                );

                return;
            }

            if (Reflect.IsNumberType(type) || Reflect.IsNullableNumberType(type))
            {
                builder.Append(obj);
                return;
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            // if a method is both final (sealed) and private it means it must be an explicit interface impl, because it
            // cannot be declared as both abstract/virtual and private
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !p.GetAccessors().All(a => a.IsFinal && a.IsPrivate))
                .ToArray();

            string typeName = obj.GetType().Name;

            if (!typeName.StartsWith("<>"))
            {
                builder.Append('<')
                    .Append(typeName)
                    .Append('>')
                    .Append(' ');
            }

            builder.Append('{');

            if (fields.Length + properties.Length <= 0)
            {
                builder.Append(' ').Append('}').Append('\n');
                return;
            }

            if (depth <= 0 && indentation > 0)
            {
                builder.Append(' ').Append(SbuGlobals.ELLIPSES).Append(' ');
            }
            else
            {
                builder.Append('\n');

                foreach (FieldInfo fieldInfo in fields)
                    appendMember(fieldInfo.GetValue(obj), fieldInfo.Name);

                foreach (PropertyInfo propertyInfo in properties)
                    appendMember(propertyInfo.SafeGetValue(obj), propertyInfo.Name);

                for (int i = 0; i < indentation; i++)
                    builder.Append(' ');
            }

            builder.Append('}');

            void appendMember(object? member, string memberName)
            {
                for (int i = 0; i < indentation + indentationDelta; i++)
                    builder.Append(' ');

                AppendObjectInspectionTo(
                    builder.Append(memberName).Append(':').Append(' '),
                    member,
                    traversedObjects,
                    depth - 1,
                    indentation + indentationDelta,
                    indentationDelta,
                    itemCount,
                    maxStringLength
                );

                builder.Append('\n');
            }
        }

        private static void AppendEnumerableInspectionTo(
            StringBuilder builder,
            IEnumerable enumerable,
            ISet<object> traversedObjects,
            int depth,
            int indentation,
            int indentationDelta,
            int itemCount,
            int maxStringLength
        )
        {
            builder.Append('[');
            int count = 0;

            if (depth <= 0 && indentation > 0)
            {
                builder.Append(' ').Append(SbuGlobals.ELLIPSES).Append(' ');
            }
            else
            {
                builder.Append('\n');

                foreach (object? obj in enumerable)
                {
                    for (int i = 0; i < indentation + indentationDelta; i++)
                        builder.Append(' ');

                    if (count >= itemCount)
                    {
                        builder.Append(SbuGlobals.ELLIPSES);
                        break;
                    }

                    AppendObjectInspectionTo(
                        builder,
                        obj,
                        traversedObjects,
                        depth - 1,
                        indentation + indentationDelta,
                        indentationDelta,
                        itemCount,
                        maxStringLength
                    );

                    builder.Append('\n');
                    count++;
                }

                if (count == 0)
                {
                    builder.Remove(builder.Length - 1, 1).Append(' ');
                }
                else
                {
                    for (int i = 0; i < indentation; i++)
                        builder.Append(' ');
                }
            }

            builder.Append(']');

            if (enumerable is ICollection collection)
                builder.Append(' ').Append('(').Append(collection.Count).Append(')');
        }

        // TODO: change to method that removes newlines if something can be one-lined
        private static bool fitsOneLine(StringBuilder builder, int appendCount)
        {
            int index = 0;

            for (int i = builder.Length - 1; i >= 0; i--)
            {
                index = i;

                if (builder[i] == '\n')
                    break;
            }

            return (builder.Length - index) + appendCount < MAX_EMBED_CODE_BLOCK_WIDTH;
        }
    }
}