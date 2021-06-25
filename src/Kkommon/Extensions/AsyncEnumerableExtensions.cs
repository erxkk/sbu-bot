using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Kkommon.Extensions.AsyncEnumerable
{
    /// <summary>
    ///     A collection of extension methods as syntactic sugar for <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        ///     Asynchronously collects an <see cref="IAsyncEnumerable{T}" /> into an <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="source">The source collection to fully collect.</param>
        /// <typeparam name="TSource">
        ///     The type of the elements in the source <see cref="IAsyncEnumerable{T}" />.
        /// </typeparam>
        /// <returns>
        ///     A <see cref="Task{T}" /> that finishes after the <see cref="IAsyncEnumerable{T}" /> has been completely
        ///     collected.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null" />.</exception>
        [Pure]
        public static async Task<IEnumerable<TSource>> CollectAsync<TSource>(
            [InstantHandle] this IAsyncEnumerable<TSource> source
        )
        {
            Preconditions.NotNull(source, nameof(source));

            List<TSource> result = new();

            await foreach (TSource item in source)
                result.Add(item);

            return result;
        }
    }
}