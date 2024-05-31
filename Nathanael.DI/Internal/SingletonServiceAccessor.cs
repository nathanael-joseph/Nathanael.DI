﻿using System;
using System.Collections.Generic;

namespace Nathanael.DI.Internal;

internal class SingletonServiceAccessor : ServiceAccessor
{
    private readonly object _lock = new();
    private readonly Func<IServiceProvider, Type?, object?> _factory;
    private readonly Dictionary<Type, object?> _instances = new();

    public override Lifetime Lifetime => Lifetime.Singleton;

    public SingletonServiceAccessor(Func<IServiceProvider, Type?, object?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        _factory = factory;
    }

    public override ServiceAccessor CreateScopedServiceAccessor()
    {
        return this;
    }

    public override object? GetService(ServiceProvider serviceProvider, Type? genericImplementationType = null)
    {
        var key = genericImplementationType ?? typeof(object);

        if (_instances.TryGetValue(key, out var instance))
        {
            return instance;
        }
        
        lock (_lock)
        {
            if (!_instances.TryGetValue(key, out instance))
            {
                instance = _factory.Invoke(serviceProvider, genericImplementationType);
                _instances[key] = instance;
            }

            return instance;
        }
    }

    public override void Dispose()
    {
        foreach (var i in _instances.Values)
        {
            if (i is IDisposable d) d.Dispose();
        }
    }
}

