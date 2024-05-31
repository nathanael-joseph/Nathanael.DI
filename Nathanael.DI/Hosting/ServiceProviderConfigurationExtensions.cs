using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Nathanael.DI.Hosting
{
    public static class ServiceProviderConfigurationExtensions
    {
        public static ServiceProviderConfiguration RegisterServiceCollection(this ServiceProviderConfiguration serviceProviderConfiguration,
                                                                             IServiceCollection serviceCollection)
        {
            foreach (var sd in serviceCollection)
            {
                ServiceConfiguration config;

                if (sd.ImplementationInstance != null)
                {
                    config = new ServiceConfiguration(sd.ServiceType, Convert(sd.Lifetime));
                    config.FactoryMethod = _ => sd.ImplementationInstance;
                }
                else
                {
                    if(sd.ServiceType == typeof(ILogger<>))
                    {

                    }

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

                serviceProviderConfiguration.ServiceConfigurations.Add(config);
            }
            return serviceProviderConfiguration;
        }

        private static Lifetime Convert(ServiceLifetime sl)
        {
            switch (sl)
            {
                case ServiceLifetime.Transient: return Lifetime.Transient;
                case ServiceLifetime.Scoped: return Lifetime.Scoped;
                case ServiceLifetime.Singleton: return Lifetime.Singleton;
                default: throw new NotImplementedException();
            }

        }
    }
}
