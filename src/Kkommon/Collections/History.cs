using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

namespace Kkommon
{
    /// <summary>
    ///     A class that represents a traversable history of items.
    /// </summary>
    /// <typeparam name="T">The type of items this history contains.</typeparam>
    [PublicAPI]
    public sealed class History<T> : ICollection<T>
    {
        private bool _empty = true;
        private readonly Stack<T> _previousHistory = new();
        private readonly Stack<T> _nextHistory = new();

        /// <inheritdoc />
        [CollectionAccess(CollectionAccessType.Read)]
        public int Count => _empty ? 1 + _previousHistory.Count + _nextHistory.Count : 0;

        /// <summary>
        ///     The a collection of items before the current item.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public IReadOnlyCollection<T> Previous => _previousHistory;

        /// <summary>
        ///     The a collection of items after the current item.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public IReadOnlyCollection<T> Next => _nextHistory;

        /// <summary>
        ///     The current element of the history.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public T? Current { get; private set; }

        /// <summary>
        ///     Moves back in the history by one item.
        /// </summary>
        /// <returns>The new current item.</returns>
        /// <exception cref="InvalidOperationException">There are no items before the current item.</exception>
        [CollectionAccess(CollectionAccessType.Read)]
        public T GoBack()
        {
            if (!TryGoBack(out T? previous))
                throw new InvalidOperationException("Could not go back, there are no elements.");

            return previous;
        }

        /// <summary>
        ///     Tries to move back in the history by one item.
        /// </summary>
        /// <returns><see langword="true"/> if there were items to move back to; otherwise <see langword="false"/>.</returns>
        [CollectionAccess(CollectionAccessType.Read)]
        public bool TryGoBack() => TryGoBack(out _);

        /// <summary>
        ///     Tries to move back in the history by one item.
        /// </summary>
        /// <param name="current">The new current item if moving back was successful.</param>
        /// <returns><see langword="true"/> if there were items to move back to; otherwise <see langword="false"/>.</returns>
        [CollectionAccess(CollectionAccessType.Read)]
        public bool TryGoBack([MaybeNullWhen(false)] out T current)
        {
            current = default;

            if (_empty)
                return false;

            if (!_previousHistory.TryPop(out T? foundPrevious))
                return false;

            _nextHistory.Push(Current!);
            Current = current = foundPrevious;
            return true;
        }

        /// <summary>
        ///     Moves forward in the history by one item.
        /// </summary>
        /// <returns>The new current item.</returns>
        /// <exception cref="InvalidOperationException">There are no items after the current item.</exception>
        [CollectionAccess(CollectionAccessType.Read)]
        public T GoForward()
        {
            if (!TryGoForward(out T? next))
                throw new InvalidOperationException("Could not go forward, there are no elements.");

            return next;
        }

        /// <summary>
        ///     Tries to move forward in the history by one item.
        /// </summary>
        /// <returns><see langword="true"/> if there were items to move forward to; otherwise <see langword="false"/>.</returns>
        [CollectionAccess(CollectionAccessType.Read)]
        public bool TryGoForward() => TryGoForward(out _);

        /// <summary>
        ///     Tries to move forward in the history by one item.
        /// </summary>
        /// <param name="current">The new current item if moving forward was successful.</param>
        /// <returns><see langword="true"/> if there were items to move forward to; otherwise <see langword="false"/>.</returns>
        [CollectionAccess(CollectionAccessType.Read)]
        public bool TryGoForward([MaybeNullWhen(false)] out T current)
        {
            current = default;

            if (_empty)
                return false;

            if (!_nextHistory.TryPop(out T? foundNext))
                return false;

            _previousHistory.Push(Current!);
            Current = current = foundNext;
            return true;
        }

        /// <summary>
        ///     Adds the given item to the history, making it the new current item.
        /// </summary>
        /// <remarks>
        ///     This will clear <see cref="Next"/> completely and move the <see cref="Current"/> to the
        ///     <see cref="Previous"/>.
        /// </remarks>
        /// <param name="current">The new current item.</param>
        [CollectionAccess(CollectionAccessType.UpdatedContent | CollectionAccessType.ModifyExistingContent)]
        public void Add(T? current)
        {
            if (!_empty)
                _previousHistory.Push(Current!);

            Current = current;
            _nextHistory.Clear();
            _empty = false;
        }

        // TODO: implement
        [Obsolete("This Operation is not supported.")]
        [CollectionAccess(CollectionAccessType.Read)]
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        public void Clear()
        {
            Current = default;
            _nextHistory.Clear();
            _previousHistory.Clear();
            _empty = true;
        }

        /// <inheritdoc />
        [CollectionAccess(CollectionAccessType.Read)]
        public bool Contains(T? item)
        {
            if (_empty)
                return false;

            return Current!.Equals(item) || _nextHistory.Contains(item!) || _previousHistory.Contains(item!);
        }

        // TODO: implement
        [Obsolete("This Operation is not supported.")]
        [CollectionAccess(CollectionAccessType.Read)]
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();

        /// <inheritdoc />
        [CollectionAccess(CollectionAccessType.Read)]
        bool ICollection<T>.IsReadOnly => false;

        /// <inheritdoc />
        [CollectionAccess(CollectionAccessType.Read)]
        public IEnumerator<T> GetEnumerator()
        {
            if (_empty)
                yield break;

            foreach (T previous in _previousHistory.Reverse())
                yield return previous;

            yield return Current!;

            foreach (T next in _nextHistory)
                yield return next;
        }

        /// <inheritdoc />
        [CollectionAccess(CollectionAccessType.Read)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}