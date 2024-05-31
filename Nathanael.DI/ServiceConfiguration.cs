using System;
using System.Collections.Generic;
using System.Linq;

namespace Nathanael.DI;

public class ServiceConfiguration
{
    protected HashSet<Type> RegisteredServiceTypes { get; }
    public Type ServiceImplementationType { get; }
    public Lifetime Lifetime { get; }
    public Func<IServiceProvider, object?>? FactoryMethod { get; set; }

    public ServiceConfiguration(Type serviceImplementationType, Lifetime lifetime, IEnumerable<Type> serviceTypes, Func<IServiceProvider, object?>? factoryMethod)
    {
        ArgumentNullException.ThrowIfNull(serviceImplementationType, nameof(serviceImplementationType));
        ArgumentNullException.ThrowIfNull(lifetime, nameof(lifetime));
        ArgumentNullException.ThrowIfNull(serviceTypes, nameof(serviceTypes));

        ServiceImplementationType = serviceImplementationType;
        Lifetime = lifetime;
        RegisteredServiceTypes = serviceTypes.ToHashSet();
        FactoryMethod = factoryMethod;
    }

    public ServiceConfiguration(Type serviceType, Lifetime lifetime, Func<IServiceProvider, object?>? factoryMethod, params Type[] dependencyTypes)
        : this(serviceType, lifetime, dependencyTypes, factoryMethod)
    {
    }

    public ServiceConfiguration(Type serviceType, Lifetime lifetime, IEnumerable<Type> dependencyTypes)
        : this(serviceType, lifetime, dependencyTypes, null)
    {
    }

    public ServiceConfiguration(Type serviceType, Lifetime lifetime, Func<IServiceProvider, object?>? factoryMethod)
        : this(serviceType, lifetime, Enumerable.Empty<Type>(), factoryMethod)
    {
    }

    public ServiceConfiguration(Type serviceType, Lifetime lifetime)
        : this(serviceType, lifetime, Enumerable.Empty<Type>())
    {
    }

    public ServiceConfiguration RegisterServiceType(Type serviceType)
    {
        if (ServiceImplementationType == serviceType) return this;

        if (ServiceImplementationType.IsGenericTypeDefinition)
        {
            if (serviceType.IsInterface)
            {
                var interfaces = ServiceImplementationType.GetInterfaces();
                var found = interfaces.Any(i => i == serviceType 
                                             || (i.IsGenericType && i.GetGenericTypeDefinition() == serviceType));
                if (!found)
                {
                    throw new InvalidOperationException($"{ServiceImplementationType} does not implement {serviceType}");
                }
            }
            else if(!serviceType.IsSubclassOf(ServiceImplementationType))
            {
                throw new InvalidOperationException($"{ServiceImplementationType} does not derive from {serviceType}");
            }
        }
        else if (!ServiceImplementationType.IsAssignableTo(serviceType))
        {
            throw new InvalidOperationException($"Cannot assign {ServiceImplementationType} to instance of {serviceType}");
        }

        RegisteredServiceTypes.Add(serviceType);

        return this;
    }

    public ICollection<Type> GetServiceTypes()
    {
        if (RegisteredServiceTypes.Any())
        {
            return RegisteredServiceTypes.ToArray();
        }

        return new Type[] { ServiceImplementationType };
    }
}
