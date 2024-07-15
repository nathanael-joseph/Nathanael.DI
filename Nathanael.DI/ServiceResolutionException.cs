using System;

namespace Nathanael.DI
{
    public class ServiceResolutionException : Exception
    {
        public ServiceResolutionException()
        {
        }

        public ServiceResolutionException(string? message) : base(message)
        {
        }
    }
}
