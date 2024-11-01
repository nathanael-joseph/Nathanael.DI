using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nathanael.DI.Hosting
{
    public static class HostingExtensions
    {
        public static ServiceProviderConfiguration UseNathanaelDI(this IHostBuilder host)
        {
            ServiceProviderFactory spf = new();
            
            spf.ServiceProviderConfiguration.ProvideSingleton<ServiceScopeFactory>()
                                            .WhenResolving<IServiceScopeFactory>();

            host.UseServiceProviderFactory(spf);

            return spf.ServiceProviderConfiguration;
        }

        public static ServiceProviderConfiguration RegisterServiceCollection(this ServiceProviderConfiguration serviceProviderConfiguration,
                                                                             IServiceCollection serviceCollection)
        {
            foreach (var sd in serviceCollection)
            {
                ServiceConfiguration config = ToServiceConfiguration(sd);
                serviceProviderConfiguration.ServiceConfigurations.Add(config);
            }
            return serviceProviderConfiguration;
        }

        private static ServiceConfiguration ToServiceConfiguration(ServiceDescriptor sd)
        {
            ServiceConfiguration config;

            if (sd.ImplementationInstance != null)
            {
                config = new ServiceConfiguration(sd.ServiceType, Convert(sd.Lifetime));
                config.FactoryMethod = _ => sd.ImplementationInstance;
            }
            else
            {
                config = new ServiceConfiguration(sd.ImplementationType ?? sd.ServiceType, Convert(sd.Lifetime));

                if (sd.ServiceType != config.ServiceImplementationType)
                {
                    config.RegisterServiceType(sd.ServiceType);
                }

                if (sd.ImplementationFactory != null)
                {
                    config.FactoryMethod = sd.ImplementationFactory;
                }
            }

            return config;
        }

        private static Lifetime Convert(ServiceLifetime sl)
        {
            return sl switch
            {
                ServiceLifetime.Transient => Lifetime.Transient,
                ServiceLifetime.Scoped => Lifetime.Scoped,
                ServiceLifetime.Singleton => Lifetime.Singleton,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
