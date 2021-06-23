using System;
using System.Runtime.Serialization;

namespace Stream.Core
{
    public class StreamCoreException : Exception
    {
#nullable enable
        public StreamCoreException() : base("Stream puller core exception.") { }
        public StreamCoreException(string? message) : base(message) { }

        public StreamCoreException(string? message, Exception? innerException) : base(message, innerException) { }

        protected StreamCoreException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#nullable disable
    }
}
