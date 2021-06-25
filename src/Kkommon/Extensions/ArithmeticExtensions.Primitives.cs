using System;

using JetBrains.Annotations;

namespace Kkommon.Extensions.Arithmetic
{
    public static partial class ArithmeticExtensions
    {
        /// <summary>
        ///     Checks if this value is inside a given range.
        /// </summary>
        /// <remarks>
        ///     This range check is always inclusive, and the given range must be satisfy a..b where a &lt;= b.
        /// </remarks>
        /// <param name="this">This value.</param>
        /// <param name="range">The range to check against.</param>
        /// <returns>
        ///     <see langword="true" /> if this value is in the given range; <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsInRange(
            this int @this,
            Range range
        ) => @this >= (range.Start.IsFromEnd ? int.MinValue : range.Start.Value)
            && @this <= (range.End.IsFromEnd ? int.MaxValue : range.End.Value);

        /// <summary>
        ///     Checks if this value is inside a given range.
        /// </summary>
        /// <remarks>
        ///     This range check is always inclusive.
        /// </remarks>
        /// <param name="this">This value.</param>
        /// <param name="lowerBound">The lower bound to check against.</param>
        /// <param name="upperBound">The upper bound to check against.</param>
        /// <returns>
        ///     <see langword="true" /> if this value is in the given range; <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsInRange(
            this ulong @this,
            ulong lowerBound,
            ulong upperBound
        ) => @this >= lowerBound && @this <= upperBound;
    }
}