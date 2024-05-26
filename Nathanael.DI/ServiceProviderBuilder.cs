using Microsoft.Extensions.DependencyInjection;
using Nathanael.DI.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            var factory = BuildFactoryMethod(sc, configuration);

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
            if (sc.FactoryMethod != null) return sc.FactoryMethod;
            if (sc.ServiceType.IsPrimitive) return sp => Activator.CreateInstance(sc.ServiceType);
            if (sc.ServiceType.IsAbstract) throw new ServiceConfigurationException($"The type {sc.ServiceType} is an abstract type. A service configuration for an abstract service type must define a factory method.");

            var ci = GetConstructorInfo(sc.ServiceType);

            return BuildFactoryMethod(ci, configuration);
        }

        private Func<IServiceProvider, object?> BuildFactoryMethod(ConstructorInfo ci, ServiceProviderConfiguration configuration)
        {
            var parameters = GetConstructorParameters(ci, configuration);

            var iServiceProvider = typeof(IServiceProvider);
            var getServiceMethod = iServiceProvider.GetMethod(nameof(IServiceProvider.GetService))!;
            var getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions).GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), 
                                                                                              BindingFlags.Public | BindingFlags.Static,
                                                                                              new[] { iServiceProvider, typeof(Type) })!;

            /* 
             * the expression we are defining is as follows:
             * sp => new Service((Dependency1)sp.GetRequiredService(dependencyType1), // for non nullable parameters
             *                   (Dependency2)sp.GetService(dependencyType2),  // for nullable parameters
             *                   ... , 
             *                   (DependencyN)sp.GetService(dependencyTypeN));
             */

            var nc = new NullabilityInfoContext();
            var sp = Expression.Parameter(iServiceProvider);
            var parameterCalls = parameters.Select(p => Expression.Convert(nc.Create(p).WriteState == NullabilityState.NotNull ?
                                                                               Expression.Call(getRequiredServiceMethod, sp, Expression.Constant(p.ParameterType)) :
                                                                               Expression.Call(sp, getServiceMethod, Expression.Constant(p.ParameterType)), 
                                                                           p.ParameterType));

            var createService = Expression.New(ci, arguments: parameterCalls);
            var factory = Expression.Lambda<Func<IServiceProvider, object?>>(createService, sp);
            
            return factory.Compile();
        }

        private static ParameterInfo[] GetConstructorParameters(ConstructorInfo ci, ServiceProviderConfiguration configuration)
        {
            var parameters = ci.GetParameters();
            
            if(parameters.Any()) 
            {
                var resolvableTypoes = configuration.ServiceConfigurations.SelectMany(sc => sc.GetDependencyTypes());

                foreach (var p in parameters)
                {
                    if (p.ParameterType.IsGenericTypeParameter)
                    {
                        throw new ServiceConfigurationException($"Cannot resolve dependency {p.ParameterType} required to create service {ci.DeclaringType} as it is a generic type parameter.");
                    }

                    if (!resolvableTypoes.Contains(p.ParameterType))
                    {
                        throw new ServiceConfigurationException($"Cannot resolve dependency {p.ParameterType} required to create service {ci.DeclaringType} as no service has been configured for it.");
                    }
                }
            }

            return parameters;
        }

        private ConstructorInfo GetConstructorInfo(Type serviceType)
        {
            var constructors = serviceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Length > 1)
            {
                var ci = constructors.FirstOrDefault(c => c.GetCustomAttribute<DIConstructorAttribute>() != null);
                if (ci == null) throw new ServiceConfigurationException($"The type {serviceType} defines multiple constructors. If a service type defines multiple public constructors, the constructor which the service provider should use must be marked with a 'DIConstructorAttribute'.");
                return ci;
            }

            if (constructors.Length == 0) throw new ServiceConfigurationException($"The type {serviceType} does not define any public constructors.");

            return constructors[0];
        }
    }
}
