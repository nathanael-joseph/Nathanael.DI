using System;
using System.Linq;
using System.Reflection;

namespace Nathanael.DI.Internal
{
    internal static class ConstructorSelector
    {
        public static ConstructorInfo SelectConstructor(ServiceConfiguration serviceConfiguration, ResolvableDependencyCollection resolvableDependenies)
        {
            return SelectConstructor(serviceConfiguration.ServiceImplementationType,
                                     serviceConfiguration.Lifetime,
                                     resolvableDependenies);
        }

        public static ConstructorInfo SelectConstructor(Type serviceType, 
                                                        Lifetime serviceLifetime,
                                                        ResolvableDependencyCollection resolvableDependenies)
        {
            var constructors = serviceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Length == 0) throw new ServiceConfigurationException($"The type {serviceType} does not define any public constructors.");

            var ci = constructors.FirstOrDefault(c => c.GetCustomAttribute<DIConstructorAttribute>() != null);

            return ci ?? GetConstructorWithMostResolvableParameters(constructors, serviceLifetime, resolvableDependenies);
        }

        private static ConstructorInfo GetConstructorWithMostResolvableParameters(ConstructorInfo[] constructors,
                                                                                  Lifetime serviceLifetime,
                                                                                  ResolvableDependencyCollection resolvableDependenies)
        {
            var constructor = constructors[0];
            var hits = constructor.GetParameters().Count(pi => resolvableDependenies.CanResolveDependencyType(serviceLifetime, pi.ParameterType));

            foreach (var c in constructors.Skip(1))
            {
                var h = c.GetParameters().Count(pi => resolvableDependenies.CanResolveDependencyType(serviceLifetime, pi.ParameterType));
                if (h > hits)
                {
                    constructor = c;
                    hits = h;
                }
            }

            return constructor;
        }
    }
}
