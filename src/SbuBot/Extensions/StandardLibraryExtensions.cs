using System.ComponentModel;
using System.Text;

using Disqord;

using SbuBot.Inspection;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class StandardLibraryExtensions
    {
        public static string GetInspection(this object @object, int maxDepth = 1)
        {
            StringBuilder builder = new(LocalEmbed.MAX_DESCRIPTION_LENGTH / 2, LocalEmbed.MAX_DESCRIPTION_LENGTH);
            Inspect.AppendInspectionTo(builder, @object, maxDepth);
            return builder.ToString();
        }
    }
}