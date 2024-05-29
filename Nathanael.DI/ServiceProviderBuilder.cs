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
            var serviceLifetimes = GetServiceLifetimes(configuration.ServiceConfigurations);

            var serviceConfigurationAcessors = configuration.ServiceConfigurations
                                                            .Select(sc => (ServiceConfiguration: sc, ServiceAccessor: BuildServiceAccessor(sc, serviceLifetimes)))
                                                            .ToList();

            var serviceAccessorDictionary = new ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>>();

            foreach (var st in serviceLifetimes.Keys)
            {
                var accessors = serviceConfigurationAcessors.Where(sca => sca.ServiceConfiguration.GetServiceTypes().Contains(st))
                                                            .Select(sca => sca.ServiceAccessor)
                                                            .ToList();

                serviceAccessorDictionary[st] = accessors;
            }

            return serviceAccessorDictionary;
        }

        private Dictionary<Type, Lifetime> GetServiceLifetimes(List<ServiceConfiguration> serviceConfigurations)
        {
            var serviceTypeLifetimes = serviceConfigurations.SelectMany(sc => sc.GetServiceTypes().Select(st => (ServiceType: st, Lifetime: sc.Lifetime)))
                                                            .GroupBy(sl => sl.ServiceType);

            foreach (var group in serviceTypeLifetimes)
            {
                var lifetimes = group.Select(g => g.Lifetime).Distinct();
                if (lifetimes.Count() != 1)
                {
                    throw new ServiceConfigurationException($"The service type {group.Key} cannot be registered with {lifetimes.First()} and with {lifetimes.Last()}. A resolvable service type can only be registered with one lifetime.");
                }
            }

            return serviceTypeLifetimes.ToDictionary(stl => stl.Key, stl => stl.First().Lifetime);

        }

        private ServiceAccessor BuildServiceAccessor(ServiceConfiguration sc, Dictionary<Type, Lifetime> serviceLifetimes)
        {
            var factory = BuildFactoryMethod(sc, serviceLifetimes);

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

        private Func<IServiceProvider, object?> BuildFactoryMethod(ServiceConfiguration sc, Dictionary<Type, Lifetime> serviceLifetimes)
        {
            if (sc.FactoryMethod != null) return sc.FactoryMethod;
            if (sc.ServiceImplementationType.IsPrimitive) return sp => Activator.CreateInstance(sc.ServiceImplementationType);
            if (sc.ServiceImplementationType.IsAbstract) throw new ServiceConfigurationException($"The type {sc.ServiceImplementationType} is an abstract type. A service configuration for an abstract service type must define a factory method.");

            var ci = GetConstructorInfo(sc.ServiceImplementationType);

            return BuildFactoryMethod(ci, sc, serviceLifetimes);
        }

        private Func<IServiceProvider, object?> BuildFactoryMethod(ConstructorInfo ci, ServiceConfiguration sc, Dictionary<Type, Lifetime> serviceLifetimes)
        {
            var parameters = GetConstructorParameters(ci, sc.Lifetime, serviceLifetimes);

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

            var sp = Expression.Parameter(iServiceProvider);
            var parameterCalls = parameters.Select(p => Expression.Convert(p.Required ?
                                                                               Expression.Call(getRequiredServiceMethod, sp, Expression.Constant(p.ParameterInfo.ParameterType)) :
                                                                               Expression.Call(sp, getServiceMethod, Expression.Constant(p.ParameterInfo.ParameterType)), 
                                                                           p.ParameterInfo.ParameterType));

            var createService = Expression.New(ci, arguments: parameterCalls);
            var factory = Expression.Lambda<Func<IServiceProvider, object?>>(createService, sp);
            
            return factory.Compile();
        }

        private static (ParameterInfo ParameterInfo, bool Required)[] GetConstructorParameters(ConstructorInfo ci, Lifetime lifetime, Dictionary<Type, Lifetime> serviceLifetimes)
        {
            var nc = new NullabilityInfoContext();

            var parameters = ci.GetParameters()
                               .Select(pi => (ParameterInfo: pi, Required: nc.Create(pi).WriteState != NullabilityState.Nullable))
                               .ToArray();

            foreach (var p in parameters)
            {
                var type = p.ParameterInfo.ParameterType;

                if(p.Required)
                {
                    if (type.IsGenericTypeParameter)
                    {
                        throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {ci.DeclaringType} as it is a generic type parameter.");
                    }

                    if (!serviceLifetimes.Keys.Contains(type))
                    {
                        throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {ci.DeclaringType} as no service has been configured for it.");
                    }
                }

                if (lifetime == Lifetime.Singleton && serviceLifetimes.ContainsKey(type) && serviceLifetimes[type] == Lifetime.Scoped)
                {
                    throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {ci.DeclaringType} with lifetime {lifetime} as the dependency {type} is registered as {Lifetime.Scoped}");
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
