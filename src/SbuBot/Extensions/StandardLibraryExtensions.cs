using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Disqord;

using Kkommon;

using SbuBot.Evaluation.Inspection;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class StandardLibraryExtensions
    {
        public static string TrimOrSelf(this string @this, int length)
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.Greater(length, 1, nameof(length));

            if (@this.Length <= length)
                return @this;

            return @this[..(length - 1)] + SbuGlobals.ELLIPSES;
        }

        public static string GetInspection(
            this object? @object,
            int maxDepth = 1,
            int indentationDelta = 2,
            int itemCount = 5
        )
        {
            StringBuilder builder = new(LocalEmbed.MaxDescriptionLength);
            Inspect.AppendInspectionTo(builder, @object, maxDepth, indentationDelta, itemCount);
            return builder.ToString();
        }

        public static string ToNewLines(this IEnumerable<string> @this) => string.Join('\n', @this);
    }
}
