using System;
using System.Runtime.Serialization;

using JetBrains.Annotations;

namespace Kkommon.Exceptions
{
    /// <summary>
    ///     An exception to throw in unit tests.
    /// </summary>
    [PublicAPI]
    public sealed class UnitTestException : Exception
    {
        /// <inheritdoc />
        public UnitTestException() { }

        /// <inheritdoc />
        public UnitTestException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <inheritdoc />
        public UnitTestException(string? message) : base(message) { }

        /// <inheritdoc />
        public UnitTestException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}