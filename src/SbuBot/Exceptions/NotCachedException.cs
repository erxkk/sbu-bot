using System;
using System.Runtime.Serialization;

namespace SbuBot.Exceptions
{
    public sealed class NotCachedException : Exception
    {
        public NotCachedException() { }

        public NotCachedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public NotCachedException(string? message) : base(message) { }

        public NotCachedException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}