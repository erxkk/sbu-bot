using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SbuBot.Evaluation.Inspection
{
    public static partial class Inspect
    {
        public static void AppendEnumInspectionTo(
            StringBuilder builder,
            Enum @enum,
            int maxDepth,
            int indentationDelta = 2,
            int itemCount = 5
        ) => Inspect.AppendEnumInspectionTo(
            builder,
            @enum,
            new HashSet<object>(),
            maxDepth,
            0,
            indentationDelta,
            itemCount
        );

        private static void AppendEnumInspectionTo(
            StringBuilder builder,
            Enum @enum,
            ISet<object> traversedObjects,
            int maxDepth,
            int indentation,
            int indentationDelta,
            int itemCount
        )
        {
            Type type = @enum.GetType();

            if (!type.GetCustomAttributes().OfType<FlagsAttribute>().Any())
            {
                builder.Append(' ').Append(@enum);
                return;
            }

            builder.Append(type.Name).Append(' ').AppendLine(@enum.ToString("X"));

            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);

            int count = 0;

            for (int i = 0; i < names.Length; i++)
            {
                object value = values.GetValue(i)!;
                ulong ulongValue = ((IConvertible)value).ToUInt64(null);

                // discard all values that are not powers of 2
                if ((ulongValue) == 0 || (ulongValue & (ulongValue - 1)) != 0)
                    continue;

                if (count >= itemCount)
                {
                    builder.Append(SbuGlobals.ELLIPSES).Append('\n');
                    break;
                }

                for (int j = 0; j < indentation + indentationDelta; j++)
                    builder.Append(' ');

                builder.Append('|')
                    .Append(' ')
                    .Append(@enum.HasFlag((Enum)value) ? SbuGlobals.BULLET : ' ')
                    .Append(' ')
                    .AppendLine(names[i]);

                count++;
            }

            if (count != 0)
                builder.Remove(builder.Length - 1, 1);
        }
    }
}