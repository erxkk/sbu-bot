using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Kkommon.Exceptions;

namespace SbuBot.Evaluation.Inspection
{
    public static partial class Inspect
    {
        public static void AppendInspectionTo(
            StringBuilder builder,
            object obj,
            int maxDepth,
            int indentationDelta = 2,
            int itemCount = 5
        ) => AppendInspectionTo(
            builder,
            obj,
            new HashSet<object>(),
            maxDepth,
            0,
            indentationDelta,
            itemCount
        );

        private static void AppendInspectionTo(
            StringBuilder builder,
            object? obj,
            ISet<object> traversedObjects,
            int maxDepth,
            int indentation,
            int indentationDelta,
            int itemCount
        )
        {
            if (obj is null)
            {
                builder.Append("null");
                return;
            }

            Type type = obj.GetType();

            while (EXTRACTORS.TryGetValue(
                type.IsGenericType ? type.GetGenericTypeDefinition() : type,
                out var extractor
            ))
            {
                Extraction extraction = extractor(obj!);

                switch (extraction.Type)
                {
                    case ExtractionType.None:
                        goto lmao; // why yes i use goto how could you tell

                    case ExtractionType.Literal:
                        builder.Append(extraction.Value);
                        return;

                    case ExtractionType.Extracted when extraction.Value is null:
                        builder.Append("null");
                        return;

                    case ExtractionType.Extracted:
                        obj = extraction.Value;
                        type = obj.GetType();
                        break;

                    default:
                        throw new UnreachableException();
                }
            }

            lmao: // used as bail out inside the loop switch

            if (!Reflect.IsValueComparable(type) && !traversedObjects.Add(obj))
            {
                builder.Append("{@}");
                return;
            }

            switch (obj)
            {
                case Enum @enum:
                {
                    AppendEnumInspectionTo(
                        builder,
                        @enum,
                        traversedObjects,
                        maxDepth,
                        indentation,
                        indentationDelta,
                        itemCount
                    );

                    break;
                }

                case IEnumerable enumerable:
                {
                    AppendEnumerableInspectionTo(
                        builder,
                        enumerable,
                        traversedObjects,
                        maxDepth,
                        indentation,
                        indentationDelta,
                        itemCount
                    );

                    break;
                }

                default:
                {
                    AppendObjectInspectionTo(
                        builder,
                        obj,
                        traversedObjects,
                        maxDepth,
                        indentation,
                        indentationDelta,
                        itemCount
                    );

                    break;
                }
            }
        }
    }
}