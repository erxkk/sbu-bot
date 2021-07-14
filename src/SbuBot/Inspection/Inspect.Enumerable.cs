using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SbuBot.Inspection
{
    public static partial class Inspect
    {
        public static void AppendEnumerableInspectionTo(
            StringBuilder builder,
            IEnumerable enumerable,
            int maxDepth,
            int indentationDelta = 2,
            int itemCount = 5
        ) => AppendEnumerableInspectionTo(
            builder,
            enumerable,
            new HashSet<object>(),
            maxDepth,
            0,
            indentationDelta,
            itemCount
        );

        private static void AppendEnumerableInspectionTo(
            StringBuilder builder,
            IEnumerable enumerable,
            ISet<object> traversedObjects,
            int maxDepth,
            int indentation,
            int indentationDelta,
            int itemCount
        )
        {
            builder.Append('[');
            int count = 0;

            if (enumerable is ICollection { Count: 0 })
            {
                builder.Append(' ').Append(']');
                return;
            }

            if (maxDepth <= 0 && indentation > 0)
            {
                builder.Append(' ').Append(SbuGlobals.ELLIPSES).Append(' ').Append(']');
                return;
            }

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

                AppendInspectionTo(
                    builder,
                    obj,
                    traversedObjects,
                    maxDepth - 1,
                    indentation + indentationDelta,
                    indentationDelta,
                    itemCount
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

            builder.Append(']');

            if (enumerable is ICollection { Count: > 0 } collection)
                builder.Append(' ').Append('(').Append(collection.Count).Append(')');
        }
    }
}