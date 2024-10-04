using System;
using System.Collections.Generic;
using System.Linq;

namespace Nathanael.DI;

/// <summary>
/// Defines a service implemntation which the service provider should provide, and the services for which it should be provided.
/// </summary>
public class ServiceConfiguration
{
    /// <summary>
    /// The service types which will be resolved by an instance of this service.
    /// </summary>
    protected HashSet<Type> RegisteredServiceTypes { get; }
    /// <summary>
    /// The service's implementation type.
    /// </summary>
    public Type ServiceImplementationType { get; }
    /// <summary>
    /// The service's lifetime.
    /// </summary>
    public Lifetime Lifetime { get; }
    /// <summary>
    /// A custom factory method for instantiating the service.
    /// </summary>
    public Func<IServiceProvider, object?>? FactoryMethod { get; set; }

    /// <summary>
    /// </summary>
    /// <param name="serviceImplementationType"></param>
    /// <param name="lifetime"></param>
    /// <param name="serviceTypes"></param>
    /// <param name="factoryMethod"></param> 
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

    /// <summary>
    /// </summary>
    /// <param name="serviceImplementationType"></param>
    /// <param name="lifetime"></param>
    /// <param name="factoryMethod"></param>
    /// <param name="serviceTypes"></param>
    public ServiceConfiguration(Type serviceImplementationType, Lifetime lifetime, Func<IServiceProvider, object?>? factoryMethod, params Type[] serviceTypes)
        : this(serviceImplementationType, lifetime, serviceTypes, factoryMethod)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="serviceImplementationType"></param>
    /// <param name="lifetime"></param>
    /// <param name="serviceTypes"></param>
    public ServiceConfiguration(Type serviceImplementationType, Lifetime lifetime, IEnumerable<Type> serviceTypes)
        : this(serviceImplementationType, lifetime, serviceTypes, null)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="serviceImplementationType"></param>
    /// <param name="lifetime"></param>
    /// <param name="factoryMethod"></param>
    public ServiceConfiguration(Type serviceImplementationType, Lifetime lifetime, Func<IServiceProvider, object?>? factoryMethod)
        : this(serviceImplementationType, lifetime, Enumerable.Empty<Type>(), factoryMethod)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="serviceImplementationType"></param>
    /// <param name="lifetime"></param>
    public ServiceConfiguration(Type serviceImplementationType, Lifetime lifetime)
        : this(serviceImplementationType, lifetime, Enumerable.Empty<Type>())
    {
    }

    /// <summary>
    /// Registers a service type which the service provider should resolve using this service.
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ServiceConfiguration RegisterServiceType(Type serviceType)
    {
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

    /// <summary>
    /// Gets the service types which the service provider should resolve using this service.
    /// </summary>
    /// <returns></returns>
    public ICollection<Type> GetServiceTypes()
    {
        if (RegisteredServiceTypes.Any())
        {
            return RegisteredServiceTypes.ToArray();
        }

        return new Type[] { ServiceImplementationType };
    }
}
