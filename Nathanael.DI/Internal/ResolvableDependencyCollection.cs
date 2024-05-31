using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nathanael.DI.Internal
{
    internal class ResolvableDependencyCollection : IEnumerable<(Type Type, bool IsScoped)>
    {
        private readonly Dictionary<Type, bool> _serviceIsScoped;
        private readonly NullabilityInfoContext _nullabilityInfoContext = new();

        public ResolvableDependencyCollection(Dictionary<Type, bool> serviceIsScoped)
        {
            _serviceIsScoped = serviceIsScoped;
        }

        public IEnumerable<(ParameterInfo ParameterInfo, bool Required)> EnsureConstructorParametersAreResolvable(ServiceConfiguration sc, ConstructorInfo constructorInfo)
        {
            return EnsureConstructorParametersAreResolvable(sc.ServiceImplementationType, sc.Lifetime, constructorInfo);
        }

        public IEnumerable<(ParameterInfo ParameterInfo, bool Required)> EnsureConstructorParametersAreResolvable(Type serviceType, Lifetime serviceLifetime, ConstructorInfo constructorInfo)
        {
            var constructorParameters = constructorInfo.GetParameters();

            var parameterInfoRequired = constructorParameters.Select(cp => EnsureConstructorParameter(serviceType, serviceLifetime, cp))
                                                             .ToArray();
            
            return parameterInfoRequired;
        }

        private (ParameterInfo ParameterInfo, bool Required) EnsureConstructorParameter(Type serviceType, Lifetime serviceLifetime, ParameterInfo pi)
        {
            var required = _nullabilityInfoContext.Create(pi).WriteState != NullabilityState.Nullable;
            var type = pi.ParameterType;

            if (required)
            {
                if (type.IsGenericTypeParameter)
                {
                    throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {serviceType} as it is a generic type parameter.");
                }

                if (!_serviceIsScoped.ContainsKey(type))
                {
                    if (type.IsGenericType)
                    {
                        var gtp = type.GetGenericTypeDefinition();

                        if (!_serviceIsScoped.ContainsKey(gtp) && gtp != typeof(IEnumerable<>))
                        {
                            throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {serviceType} as no service has been configured for it.");
                        }
                    }
                    else throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {serviceType} as no service has been configured for it.");
                }
            }

            if (serviceLifetime == Lifetime.Singleton && IsResolvableAndScoped(type))
            {
                throw new ServiceConfigurationException($"Cannot resolve dependency {type} required to create service {serviceType} with lifetime {serviceLifetime} as the dependency {type} is registered as {Lifetime.Scoped}");
            }

            return (ParameterInfo: pi, Required: required);
        }

        private bool IsResolvableAndScoped(Type type)
        {
            return (_serviceIsScoped.TryGetValue(type, out bool isScopedService) && isScopedService) || 
                   (type.IsGenericType && 
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>) && 
                    _serviceIsScoped.TryGetValue(type.GetGenericArguments()[0], out bool isScoped) && 
                    isScoped);
        }


        public bool CanResolveDependencyType(ServiceConfiguration sc, Type dependencyType)
        {
            return CanResolveDependencyType(sc.Lifetime, dependencyType);
        }

        public bool CanResolveDependencyType(Lifetime serviceLifetime, Type dependencyType)
        {
            if (dependencyType.IsGenericTypeParameter)
            {
                return false;
            }

            if (!_serviceIsScoped.ContainsKey(dependencyType))
            {
                if (dependencyType.IsGenericType)
                {
                    if (!_serviceIsScoped.ContainsKey(dependencyType.GetGenericTypeDefinition()))
                    {
                        return false;
                    }
                }
                else return false;
            }

            if (serviceLifetime == Lifetime.Singleton && _serviceIsScoped.TryGetValue(dependencyType, out bool value) && value)
            {
                return false;
            }

            return true;
        }

        public IEnumerator<(Type Type, bool IsScoped)> GetEnumerator()
        {
            return _serviceIsScoped.Select(kvp => (Type: kvp.Key, IsScoped: kvp.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
