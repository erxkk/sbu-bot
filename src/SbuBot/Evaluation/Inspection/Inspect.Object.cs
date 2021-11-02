using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SbuBot.Evaluation.Inspection
{
    public static partial class Inspect
    {
        public static void AppendObjectInspectionTo(
            StringBuilder builder,
            object obj,
            int maxDepth,
            int indentationDelta = 2,
            int itemCount = 5
        ) => Inspect.AppendObjectInspectionTo(
            builder,
            obj,
            new HashSet<object>(),
            maxDepth,
            0,
            indentationDelta,
            itemCount
        );

        private static void AppendObjectInspectionTo(
            StringBuilder builder,
            object obj,
            ISet<object> traversedObjects,
            int maxDepth,
            int indentation,
            int indentationDelta,
            int itemCount
        )
        {
            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            // if a method is both final (sealed) and private it means it must be an explicit interface impl, because it
            // cannot be declared as both abstract/virtual and private
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && !p.GetAccessors().All(a => a.IsFinal && a.IsPrivate))
                .ToArray();

            if (!type.Name.StartsWith("<>f__AnonymousType"))
            {
                builder.Append('<')
                    .Append(type.Name)
                    .Append('>')
                    .Append(' ');
            }

            builder.Append('{');

            if (fields.Length + properties.Length <= 0)
            {
                builder.Append(' ').Append('}');
                return;
            }

            if (maxDepth <= 0 && indentation > 0)
            {
                builder.Append(' ').Append(SbuGlobals.ELLIPSES).Append(' ');
            }
            else
            {
                builder.Append('\n');

                foreach (FieldInfo fieldInfo in fields)
                    appendMember(fieldInfo.GetValue(obj), fieldInfo.Name);

                foreach (PropertyInfo propertyInfo in properties)
                    appendMember(Inspect.SafeGetProperty(propertyInfo, obj), propertyInfo.Name);

                for (int i = 0; i < indentation; i++)
                    builder.Append(' ');
            }

            builder.Append('}');

            void appendMember(object? member, string memberName)
            {
                for (int i = 0; i < indentation + indentationDelta; i++)
                    builder.Append(' ');

                AppendInspectionTo(
                    builder.Append(memberName).Append(':').Append(' '),
                    member,
                    traversedObjects,
                    maxDepth - 1,
                    indentation + indentationDelta,
                    indentationDelta,
                    itemCount
                );

                builder.Append('\n');
            }
        }
    }
}