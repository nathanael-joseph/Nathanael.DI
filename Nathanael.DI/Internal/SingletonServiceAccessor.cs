using System;

namespace Nathanael.DI.Internal;

internal class SingletonServiceAccessor : ServiceAccessor
{
    private readonly object _lock = new();
    private Func<IServiceProvider, object?> _factory;
    private object? _instance;

    public override Lifetime Lifetime => Lifetime.Singleton;

    public SingletonServiceAccessor(Func<IServiceProvider, object?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        _factory = factory;
    }
    public override ServiceAccessor CreateScopedServiceAccessor()
    {
        return this;
    }

    public override object? GetService(ServiceProvider serviceProvider)
    {

        if (_instance != null) return _instance;

        lock (_lock)
        {
            _instance ??= _factory(serviceProvider);
            return _instance;
        }
    }

    public override void Dispose()
    {
        if (_instance is IDisposable d) d.Dispose();
    }
}

