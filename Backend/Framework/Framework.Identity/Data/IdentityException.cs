using System;
using System.Runtime.Serialization;

namespace Framework.Identity.Data
{
    public class IdentityException : Exception
    {
        public IdentityException()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        public IdentityException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public IdentityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public IdentityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}