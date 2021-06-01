using System;
using System.Runtime.Serialization;

namespace SbuBot
{
    public sealed class RequiredCacheException : Exception
    {
        public RequiredCacheException() { }

        public RequiredCacheException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public RequiredCacheException(string? message) : base(message) { }

        public RequiredCacheException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}