using System;
using System.Runtime.Serialization;

namespace Nathanael.DI
{
    public class ServiceConfigurationException : Exception
    {
        public ServiceConfigurationException()
        {
        }

        public ServiceConfigurationException(string? message) : base(message)
        {
        }

        public ServiceConfigurationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ServiceConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
