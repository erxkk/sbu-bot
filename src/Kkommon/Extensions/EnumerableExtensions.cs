using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using JetBrains.Annotations;

namespace Kkommon.Extensions.Enumerable
{
    /// <summary>
    ///     A collection of extension methods as syntactic sugar for <see cref="IEnumerable{T}" />.
    /// </summary>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Enumerates the <see cref="IEnumerable{T}" /> while exposing the index of each element.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <typeparam name="TSource">The type of the elements in the source <see cref="IEnumerable{T}" />.</typeparam>
        /// <returns>
        ///     An <see cref="IEnumerable{T}" /> containing the elements and their respective indices.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="source"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OverflowException">The enumeration resulted in an overflow.</exception>
        [Pure]
        [LinqTunnel]
        public static IEnumerable<(int Index, TSource Item)> Enumerate<TSource>(this IEnumerable<TSource> source)
        {
            Preconditions.NotNull(source, nameof(source));

            return source.Select((item, index) => (index, item));
        }

        /// <summary>
        ///     Checks whether or not an <see cref="IEnumerable{T}" /> contains at least <paramref name="count" />
        ///     elements.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="count">The number of elements to check for.</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="source" /> <see cref="IEnumerable{T}" /> contained at least
        ///     <paramref name="count" /> elements; otherwise <see langowrd="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="source"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="count"/> is less than 1.</exception>
        [Pure]
        public static bool HasAtLeast<TSource>(
            [InstantHandle] this IEnumerable<TSource> source,
            int count
        )
        {
            Preconditions.NotNull(source, nameof(source));
            Preconditions.InRange(count, 1.., nameof(count));

            return source is ICollection<TSource> collection ? collection.Count >= count : source.Skip(count - 1).Any();
        }

        /// <summary>
        ///     Checks whether or not an <see cref="IEnumerable{T}" /> contains at most <paramref name="count" />
        ///     elements.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="count">The number of elements to check for.</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="source" /> <see cref="IEnumerable{T}" /> contained at most
        ///     <paramref name="count" /> elements; otherwise <see langowrd="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="source"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="count"/> is less than 1.</exception>
        [Pure]
        public static bool HasAtMost<TSource>(
            [InstantHandle] this IEnumerable<TSource> source,
            int count
        )
        {
            Preconditions.NotNull(source, nameof(source));
            Preconditions.InRange(count, 1.., nameof(count));

            return source is ICollection<TSource> collection ? collection.Count <= count : !source.Skip(count).Any();
        }
    }
}