using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nathanael.DI;

public class ServiceConfiguration
{
    protected HashSet<Type> RegisteredDependencyTypes { get; }
    public Type ServiceType { get; }
    public Lifetime Lifetime { get; }
    public Func<IServiceProvider, object?>? FactoryMethod { get; set; }

    public ServiceConfiguration(Type serviceType, Lifetime lifetime, IEnumerable<Type> dependencyTypes, Func<IServiceProvider, object?>? factoryMethod)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
        ArgumentNullException.ThrowIfNull(lifetime, nameof(lifetime));
        ArgumentNullException.ThrowIfNull(dependencyTypes, nameof(dependencyTypes));

        ServiceType = serviceType;
        Lifetime = lifetime;
        RegisteredDependencyTypes = dependencyTypes.ToHashSet();
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

    public ServiceConfiguration RegisterDependencyType(Type dependancyType)
    {
        if (!ServiceType.IsAssignableTo(dependancyType))
        {
            throw new InvalidOperationException($"Cannot assign {ServiceType} to instance of {dependancyType}");
        }

        RegisteredDependencyTypes.Add(dependancyType);

        return this;
    }

    public ICollection<Type> GetDependencyTypes()
    {
        if (RegisteredDependencyTypes.Any())
        {
            return RegisteredDependencyTypes.ToArray();
        }

        return new Type[] { ServiceType };
    }
}
