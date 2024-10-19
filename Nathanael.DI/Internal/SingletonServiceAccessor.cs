using System;
using System.Collections.Concurrent;

namespace Nathanael.DI.Internal;

internal class SingletonServiceAccessor : ServiceAccessor
{
    private readonly ConcurrentDictionary<Type, object?> _instances = new();

    public SingletonServiceAccessor(Func<IServiceProvider, Type?, object?> factory) : base(factory)
    {
    }

    public override ServiceAccessor CreateScopedServiceAccessor()
    {
        return this;
    }

    public override object? GetService(ServiceProvider serviceProvider, Type? genericImplementationType = null)
    {
        var key = genericImplementationType ?? typeof(object);

        return _instances.GetOrAdd(key, k => Factory.Invoke(serviceProvider, genericImplementationType));
    }

    public override void Dispose()
    {
        foreach (var i in _instances.Values)
        {
            if (i is IDisposable d) d.Dispose();
        }
    }
}

