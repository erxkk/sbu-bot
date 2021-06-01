using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class StandardLibraryExtensions
    {
        public static string Indent(this string @this, int count)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, null);

            StringBuilder sb = new StringBuilder(@this.Length + count * (@this.Count(c => c == '\n') + 1)).Append("  ");
            ReadOnlySpan<char> span = @this.AsSpan();

            for (var i = 0; i < span.Length; i++)
            {
                sb.Append(span[i]);

                if (span[i] == '\n')
                    sb.Append("  ");
            }

            return sb.ToString();
        }
    }
}