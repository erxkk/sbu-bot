using System;
using System.ComponentModel;
using System.Diagnostics;

using JetBrains.Annotations;

namespace Kkommon
{
    /// <summary>
    ///     A type that may contain a value of type <typeparamref name="T" />.
    /// </summary>
    /// <remarks>
    ///     <see cref="Optional{T}" /> is intended to be used where <see langword="null" /> doesn't represent the absence
    ///     of data but rather just another valid value for the type <typeparamref name="T" />.
    /// </remarks>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(Optional<T>.DebuggerDisplay) + ",nq}")]
    public readonly struct Optional<T>
    {
        /// <summary>
        ///     Whether or not <see cref="Value" /> is holding a value.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        ///     The underlying value this <see cref="Optional{T}" /> is holding, check if it is set via
        ///     <see cref="HasValue" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">The <see cref="Optional{T}" /> has no value.</exception>
        public T Value
            => HasValue ? ValueOrDefault! : throw new InvalidOperationException("The given optional was empty");

        /// <summary>
        ///     Returns the underlying <see cref="Value" /> or the default for <typeparamref name="T" /> if this
        ///     <see cref="Optional{T}" /> is empty.
        /// </summary>
        public T? ValueOrDefault { get; }

        /// <summary>
        ///     Creates a new <see cref="Optional{T}" /> with the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">The underlying value.</param>
        public Optional(T value)
        {
            HasValue = true;
            ValueOrDefault = value;
        }

        /// <summary>
        ///     Returns a string representation of this <see cref="Optional{T}" /> and it's state and value.
        /// </summary>
        /// <returns>
        ///     A string representing the current <see cref="Optional{T}" />.
        /// </returns>
        public override string ToString()
            => $"Optional<{typeof(T).Name}> {{ {(HasValue ? $"Value: {Value}" : "Empty")} }}";

        private string DebuggerDisplay => $"Optional<{typeof(T).Name}> {{ HasValue = {HasValue}, Value = {Value} }}";

        public static implicit operator Optional<T>(T? value)
            => value is not null ? new(value) : new Optional<T>();
    }

    /// <summary>
    ///     A static class containing <see cref="Optional{T}" /> factory and conversion methods.
    /// </summary>
    [PublicAPI]
    public static class Optional
    {
        /// <summary>
        ///     Creates a new non-empty <see cref="Optional{T}" /> with a given value.
        /// </summary>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new non-empty <see cref="Optional{T}" /> with the given value.
        /// </returns>
        [Pure]
        public static Optional<T> FromValue<T>(T value) => new(value);

        /// <summary>
        ///     Creates a new empty <see cref="Optional{T}" /> with a given value.
        /// </summary>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new empty <see cref="Optional{T}" /> with the given value.
        /// </returns>
        [Pure]
        public static Optional<T> Empty<T>() => new();

        /// <summary>
        ///     Creates a new <see cref="Optional{T}" /> with a given value, returns an empty <see cref="Optional{T}" />
        ///     if the value is null.
        /// </summary>
        /// <param name="value">The value to wrap in the <see cref="Optional{T}" />.</param>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new <see cref="Optional{T}" /> with the given value.
        /// </returns>
        [Pure]
        public static Optional<T> FromNullable<T>(T value)
            => value is not null ? new(value) : new Optional<T>();

        /// <summary>
        ///     Safely unwraps the given <see cref="Optional{T}" /> into a <see cref="Nullable{T}" />.
        /// </summary>
        /// <param name="optional">The <see cref="Optional{T}" /> to unwrap.</param>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new <see cref="Nullable{T}" /> with the underlying value or null if the <see cref="Optional{T}" /> was
        ///     empty.
        /// </returns>
        [Pure]
        public static T? ToNullable<T>(Optional<T> optional) where T : struct
            => optional.HasValue ? optional.Value : null;

        /// <summary>
        ///     Safely unwraps the given <see cref="Optional{T}" /> into a <see cref="T" />.
        /// </summary>
        /// <param name="optional">The <see cref="Optional{T}" /> to unwrap.</param>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new <see cref="T" /> or null if the <see cref="Optional{T}" /> was empty.
        /// </returns>
        [Pure]
        public static T? ToRefNullable<T>(Optional<T> optional) where T : class
            => optional.HasValue ? optional.Value : null;
    }

    // split definitions to satisfy the compiler, because mixing Nullable<T> T:struct with T: class does not go well
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OptionalValueExtension
    {
        /// <summary>
        ///     Safely unwraps this given <see cref="Optional{T}" /> into a <see cref="Nullable{T}" />.
        /// </summary>
        /// <param name="this">The <see cref="Optional{T}" /> to unwrap.</param>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new <see cref="Nullable{T}" /> with the underlying value or null if the <see cref="Optional{T}" /> was
        ///     empty.
        /// </returns>
        [Pure]
        public static T? ToNullable<T>(this Optional<T> @this) where T : struct => Optional.ToNullable(@this);
    }

    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OptionalRefExtension
    {
        /// <summary>
        ///     Safely unwraps this given <see cref="Optional{T}" /> into a <see cref="T" />.
        /// </summary>
        /// <param name="this">The <see cref="Optional{T}" /> to unwrap.</param>
        /// <typeparam name="T">The type of the given value.</typeparam>
        /// <returns>
        ///     A new <see cref="T" /> or null if the <see cref="Optional{T}" /> was empty.
        /// </returns>
        [Pure]
        public static T? ToNullable<T>(this Optional<T> @this) where T : class => Optional.ToRefNullable(@this);
    }
}