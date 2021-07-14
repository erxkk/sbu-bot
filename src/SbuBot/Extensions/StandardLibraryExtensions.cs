using System.ComponentModel;
using System.Text;

using Disqord;

using Kkommon;

using SbuBot.Inspection;
using SbuBot.Services;

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

        public static string GetInspection(this object @object, int maxDepth = 1)
        {
            StringBuilder builder = new(LocalEmbed.MaxDescriptionLength / 2, LocalEmbed.MaxDescriptionLength);
            Inspect.AppendInspectionTo(builder, @object, maxDepth);
            return builder.ToString();
        }
    }
}