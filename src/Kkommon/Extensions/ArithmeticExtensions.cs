using System;
using System.ComponentModel;

using JetBrains.Annotations;

namespace Kkommon.Extensions.Arithmetic
{
    /// <summary>
    ///     A collection of extension methods as syntactic sugar for common arithmetic operations.
    /// </summary>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class ArithmeticExtensions
    {
        /// <summary>
        ///     Checks whether or not a value is greater than a given value.
        /// </summary>
        /// <param name="this">The value to check for.</param>
        /// <param name="other">The other value to check against.</param>
        /// <typeparam name="T">The type of the value to check against.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if this value is greater than the given value; <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsGreaterThan<T>(this IComparable<T> @this, T other) => @this.CompareTo(other) > 0;

        /// <summary>
        ///     Checks whether or not a value is greater than or equal to a given value.
        /// </summary>
        /// <param name="this">The value to check for.</param>
        /// <param name="other">The other value to check against.</param>
        /// <typeparam name="T">The type of the value to check against.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if this value is greater than or equal to the given value;
        ///     <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsGreaterThanOrEqual<T>(this IComparable<T> @this, T other) => @this.CompareTo(other) >= 0;

        /// <summary>
        ///     Checks whether or not a value is less than a given value.
        /// </summary>
        /// <param name="this">The value to check for.</param>
        /// <param name="other">The other value to check against.</param>
        /// <typeparam name="T">The type of the value to check against.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if this value is less than the given value; <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsLessThan<T>(this IComparable<T> @this, T other) => @this.CompareTo(other) < 0;

        /// <summary>
        ///     Checks whether or not a value is less than or equal to a given value.
        /// </summary>
        /// <param name="this">The value to check for.</param>
        /// <param name="other">The other value to check against.</param>
        /// <typeparam name="T">The type of the value to check against.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if this value is less than or equal to the given value;
        ///     <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsLessThanOrEqual<T>(this IComparable<T> @this, T other) => @this.CompareTo(other) <= 0;

        /// <summary>
        ///     Checks whether or not a value is inside a given range.
        /// </summary>
        /// <param name="this">The value to check for.</param>
        /// <param name="lowerBound">The lower bound to check against.</param>
        /// <param name="upperBound">The upper bound to check against.</param>
        /// <param name="leftExclusive">Whether the given range is left exclusive; defaults to false.</param>
        /// <param name="rightExclusive">Whether the given range is right inclusive; defaults to true.</param>
        /// <typeparam name="T">The type of the value to check against.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if this value is in the given range; <see langword="false" /> if not.
        /// </returns>
        [Pure]
        public static bool IsInRange<T>(
            this IComparable<T> @this,
            T lowerBound,
            T upperBound,
            bool leftExclusive = false,
            bool rightExclusive = true
        ) => (leftExclusive ? @this.IsGreaterThan(lowerBound) : @this.IsGreaterThanOrEqual(lowerBound))
            && (rightExclusive ? @this.IsLessThan(upperBound) : @this.IsLessThanOrEqual(upperBound));
    }
}