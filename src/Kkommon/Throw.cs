using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Kkommon
{
    /// <summary>
    ///     A static class with throw helper functions.
    /// </summary>
    [PublicAPI]
    public static class Throw
    {
        /// <summary>
        ///     Throws a <see cref="ArgumentOutOfRangeException" />.
        /// </summary>
        /// <remarks>
        ///     This range check is always inclusive, and the given range must be satisfy a..b where a &lt;= b.
        /// </remarks>
        /// <param name="value">The actual value that was outside of the given range.</param>
        /// <param name="lowerBound">The lower bound of the given range.</param>
        /// <param name="upperBound">The upper bound of the given range.</param>
        /// <param name="parameterName">The name of the parameter that was empty.</param>
        /// <exception cref="ArgumentOutOfRangeException">Always.</exception>
        [DoesNotReturn]
        public static void ArgumentOutOfRange(
            int value,
            int lowerBound,
            int upperBound,
            [InvokerParameterName] string parameterName
        ) => throw new ArgumentOutOfRangeException(
            parameterName,
            value,
            $"{parameterName} must not be less than {lowerBound} or greater than {upperBound}"
        );
    }
}