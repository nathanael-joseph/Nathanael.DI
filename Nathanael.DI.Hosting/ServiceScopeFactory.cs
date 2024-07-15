using Microsoft.Extensions.DependencyInjection;
using System;

namespace Nathanael.DI.Hosting
{
    public class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceScopeFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope()
        {
            if (_serviceProvider is ServiceProvider sp)
            {
                return new ServiceScope(sp.CreateScopedServiceProvider());
            }

            throw new InvalidCastException($"IServiceProvider is not of type {typeof(ServiceProvider)}");
        }
    }
}
