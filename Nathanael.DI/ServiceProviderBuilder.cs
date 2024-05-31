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
        public ServiceProviderConfiguration Configuration { get; }

        public ServiceProviderBuilder(ServiceProviderConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ServiceProviderBuilder()
        {
            Configuration = new ServiceProviderConfiguration(); 
        }

        public ServiceProviderBuilder Configure(Action<ServiceProviderConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(configure, nameof(configure));
            configure(Configuration);
            return this;
        }

        public ServiceProvider Build(ServiceProviderConfiguration configuration)
        {
            var serviceAccessorDictionary = BuildServiceAccessorDictionary(configuration);
            return new ServiceProvider(serviceAccessorDictionary);
        }

        public ServiceProvider Build()
        {
            return Build(Configuration);
        }


        private ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>> BuildServiceAccessorDictionary(ServiceProviderConfiguration configuration)
        {
            var resolvableDependencyCollection = BuildResolvableDependencyCollection(configuration.ServiceConfigurations);

            var serviceConfigurationAcessors = configuration.ServiceConfigurations
                                                            .Select(sc => (ServiceConfiguration: sc, ServiceAccessor: BuildServiceAccessor(sc, resolvableDependencyCollection)))
                                                            .ToList();

            var serviceAccessorDictionary = new ConcurrentDictionary<Type, IEnumerable<ServiceAccessor>>();

            foreach (var st in resolvableDependencyCollection.Select(rd => rd.Type))
            {
                var accessors = serviceConfigurationAcessors.Where(sca => sca.ServiceConfiguration.GetServiceTypes().Contains(st))
                                                            .Select(sca => sca.ServiceAccessor)
                                                            .ToList();

                serviceAccessorDictionary[st] = accessors;
            }

            return serviceAccessorDictionary;
        }

        private ResolvableDependencyCollection BuildResolvableDependencyCollection(List<ServiceConfiguration> serviceConfigurations)
        {
            var serviceTypeIsScoped = serviceConfigurations.SelectMany(sc => sc.GetServiceTypes().Select(st => (ServiceType: st, Lifetime: sc.Lifetime)))
                                                           .GroupBy(sl => sl.ServiceType);

            foreach (var group in serviceTypeIsScoped)
            {
                var lifetimes = group.Select(g => g.Lifetime).Distinct();
                if (lifetimes.Contains(Lifetime.Scoped) && lifetimes.Any(l => l != Lifetime.Scoped))
                {
                    throw new ServiceConfigurationException($"The service type {group.Key} cannot be registered with {Lifetime.Scoped} and with non scoped lifetimes. If a resolvable service has a scoped implementation service, all implementing services for the resolvable type must also be scoped.");
                }
            }

            var dict = serviceTypeIsScoped.ToDictionary(stl => stl.Key, stl => stl.First().Lifetime == Lifetime.Scoped);
            return new ResolvableDependencyCollection(dict);
        }

        private ServiceAccessor BuildServiceAccessor(ServiceConfiguration sc, ResolvableDependencyCollection resolvableDependencyCollection)
        {
            var factory = BuildFactoryMethod(sc, resolvableDependencyCollection);

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

        private Func<IServiceProvider, Type?, object?> BuildFactoryMethod(ServiceConfiguration sc, ResolvableDependencyCollection resolvableDependencyCollection)
        {
            if (sc.FactoryMethod != null) return (sp, _) => sc.FactoryMethod(sp);
            if (sc.ServiceImplementationType.IsPrimitive) return (sp, _) => Activator.CreateInstance(sc.ServiceImplementationType);
            if (sc.ServiceImplementationType.IsAbstract) throw new ServiceConfigurationException($"The type {sc.ServiceImplementationType} is an abstract type. A service configuration for an abstract service type must define a factory method.");

            if (sc.ServiceImplementationType.IsGenericTypeDefinition)
            {
                return BuildGenericTypeDefinitionFactory(sc, resolvableDependencyCollection);
            }
            else
            {
                return BuildConcreteFactoryMethod(sc, resolvableDependencyCollection);
            }
        }

        private Func<IServiceProvider, Type?, object?> BuildGenericTypeDefinitionFactory(ServiceConfiguration sc, ResolvableDependencyCollection resolvableDependencyCollection)
        {
            var ci = ConstructorSelector.SelectConstructor(sc, resolvableDependencyCollection);
            resolvableDependencyCollection.EnsureConstructorParametersAreResolvable(sc, ci);

            return (sp, genericImplementationType) =>
            {
                ArgumentNullException.ThrowIfNull(genericImplementationType);
                
                if (genericImplementationType.IsAbstract || genericImplementationType.IsInterface)
                {
                    var genericArgs = genericImplementationType.GetGenericArguments();
                    genericImplementationType = sc.ServiceImplementationType.MakeGenericType(genericArgs);
                }

                var cstr = ConstructorSelector.SelectConstructor(genericImplementationType, sc.Lifetime, resolvableDependencyCollection);
                var parametersRequired = resolvableDependencyCollection.EnsureConstructorParametersAreResolvable(genericImplementationType, sc.Lifetime, ci);
                
                var dependencies = parametersRequired.Select(pr => pr.Required ? 
                                                                   sp.GetRequiredService(pr.ParameterInfo.ParameterType) : 
                                                                   sp.GetService(pr.ParameterInfo.ParameterType))
                                                     .ToArray();
                
                return cstr.Invoke(dependencies);
            };
        }

        private Func<IServiceProvider, Type?, object?> BuildConcreteFactoryMethod(ServiceConfiguration sc, ResolvableDependencyCollection resolvableDependencyCollection)
        {
            var ci = ConstructorSelector.SelectConstructor(sc, resolvableDependencyCollection); 
            var parametersRequired = resolvableDependencyCollection.EnsureConstructorParametersAreResolvable(sc, ci);

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
            var genericImplementationType = Expression.Parameter(typeof(Type), "_");

            var parameterCalls = parametersRequired.Select(p => Expression.Convert(
                                                                    p.Required ?
                                                                        Expression.Call(getRequiredServiceMethod, sp, Expression.Constant(p.ParameterInfo.ParameterType)) :
                                                                        Expression.Call(sp, getServiceMethod, Expression.Constant(p.ParameterInfo.ParameterType)), 
                                                                    p.ParameterInfo.ParameterType)
                                                          );

            var createService = Expression.New(ci, arguments: parameterCalls);
            var factory = Expression.Lambda<Func<IServiceProvider, Type?, object?>>(createService, sp, genericImplementationType);
            
            return factory.Compile();
        }

    }
}
