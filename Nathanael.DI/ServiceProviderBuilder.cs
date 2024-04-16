using Nathanael.DI.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Nathanael.DI
{
    public class ServiceProviderBuilder 
    {
        public ServiceProvider Build(ServiceProviderConfiguration configuration)
        {
            var serviceAccessorDictionary = BuildServiceAccessorDictionary(configuration);
            return new ServiceProvider(serviceAccessorDictionary);
        }

        private ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>> BuildServiceAccessorDictionary(ServiceProviderConfiguration configuration)
        {
            var serviceConfigurationAcessors = configuration.ServiceConfigurations
                                                            .Select(sc => (ServiceConfiguration: sc, ServiceAccessor: BuildServiceAccessor(sc, configuration)))
                                                            .ToList();

            var dependencyTypes = serviceConfigurationAcessors.SelectMany(sca => sca.ServiceConfiguration.GetDependencyTypes())
                                                              .Distinct();

            var serviceAccessorDictionary = new ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>>();

            foreach (var d in dependencyTypes)
            {
                var accessors = serviceConfigurationAcessors.Where(sca => sca.ServiceConfiguration.GetDependencyTypes().Contains(d))
                                                            .Select(sca => sca.ServiceAccessor)
                                                            .ToList();

                serviceAccessorDictionary[d] = accessors;
            }

            return serviceAccessorDictionary;
        }

        private ServiceAccessor BuildServiceAccessor(ServiceConfiguration sc, ServiceProviderConfiguration configuration)
        {
            var factory = sc.FactoryMethod ?? BuildFactoryMethod(sc, configuration);

            switch (sc.Lifetime)
            {
                case Lifetime.Transient:
                    return new TransientServiceAccessor(factory);
                case Lifetime.Scoped:
                    return new ScopedServiceAccessor(factory);
                case Lifetime.Singleton:
                    return new SingletonServiceAccessor(factory);
                default:
                    throw new NotImplementedException();
            }
        }

        private Func<IServiceProvider, object?> BuildFactoryMethod(ServiceConfiguration sc, ServiceProviderConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
