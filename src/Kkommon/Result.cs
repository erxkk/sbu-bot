using System.Diagnostics;

using JetBrains.Annotations;

namespace Kkommon
{
    /// <summary>
    ///     A discriminated Union that represents the return type of a function that  might fail execution gracefully,
    ///     returning <typeparamref name="TSuccess" /> on success and <typeparamref name="TError" /> on failure.
    /// </summary>
    [PublicAPI]
    public abstract class Result<TSuccess, TError>
    {
        private Result() { }

        /// <summary>
        ///     A <see cref="Result{TSuccess, TError}" /> that holds a success value.
        /// </summary>
        [PublicAPI]
        [DebuggerDisplay("{" + nameof(Success.DebuggerDisplay) + ",nq}")]
        public sealed class Success : Result<TSuccess, TError>
        {
            /// <inheritdoc />
            /// <param name="value">The success value.</param>
            public Success(TSuccess value) => Value = value;

            /// <summary>
            ///     The success value.
            /// </summary>
            public TSuccess Value { get; }

            private string DebuggerDisplay => $"Success {{ Value = {Value} }}";

            /// <inheritdoc />
            [Pure]
            public override string ToString()
                => $"Result<{typeof(TSuccess).Name}, {typeof(TError).Name}> {{ Success: {Value} }}";

            [Pure]
            public static implicit operator TSuccess(Success @this) => @this.Value;

            [Pure]
            public static implicit operator Success(TSuccess value) => new(value);
        }

        /// <summary>
        ///     A <see cref="Result{TSuccess, TError}" /> that holds an error value.
        /// </summary>
        [PublicAPI]
        [DebuggerDisplay("{" + nameof(Error.DebuggerDisplay) + ",nq}")]
        public sealed class Error : Result<TSuccess, TError>
        {
            /// <inheritdoc />
            /// <param name="value">The error value</param>
            public Error(TError value) => Value = value;

            /// <summary>
            ///     The error value.
            /// </summary>
            public TError Value { get; }

            private string DebuggerDisplay => $"Error {{ Value = {Value} }}";

            /// <inheritdoc />
            [Pure]
            public override string ToString()
                => $"Result<{typeof(TSuccess).Name}, {typeof(TError).Name}> {{ Error: {Value} }}";

            [Pure]
            public static implicit operator TError(Error @this) => @this.Value;

            [Pure]
            public static implicit operator Error(TError value) => new(value);
        }
    }
}