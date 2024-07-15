using Microsoft.Extensions.DependencyInjection;
using System;

namespace Nathanael.DI.Hosting
{
    public class ServiceProviderFactory : IServiceProviderFactory<ServiceProviderBuilder>
    {
        public ServiceProviderConfiguration ServiceProviderConfiguration { get; }

        public ServiceProviderFactory(ServiceProviderConfiguration configuration)
        {
            ServiceProviderConfiguration = configuration;
        }

        public ServiceProviderFactory()
        {
            ServiceProviderConfiguration = new();
        }

        public ServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            return new ServiceProviderBuilder(ServiceProviderConfiguration).Configure(config =>
            {
                config.RegisterServiceCollection(services);
            });
        }

        public IServiceProvider CreateServiceProvider(ServiceProviderBuilder containerBuilder)
        {
            return containerBuilder.Build();
        }
    }
}
