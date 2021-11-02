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
            builder.Append(type.Name).Append(' ').Append(@enum.ToString("X"));

            if (!type.GetCustomAttributes().OfType<FlagsAttribute>().Any())
                return;

            builder.Append('\n');

            string[] names = Enum.GetNames(type);
            Array values = Enum.GetValues(type);

            int count = 0;

            for (int i = 0; i < names.Length; i++)
            {
                if (!@enum.HasFlag((Enum)values.GetValue(i)!))
                    continue;

                if (count >= itemCount)
                {
                    builder.Append(SbuGlobals.ELLIPSES).Append('\n');
                    break;
                }

                for (int j = 0; j < indentation + indentationDelta; j++)
                    builder.Append(' ');

                builder.Append('|').Append(' ').Append(names[i]).Append('\n');
                count++;
            }

            if (count != 0)
                builder.Remove(builder.Length - 1, 1);
        }
    }
}