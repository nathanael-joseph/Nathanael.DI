using System;

namespace Nathanael.DI.Internal;

internal abstract class ServiceAccessor : IDisposable
{
    protected Func<IServiceProvider, Type?, object?> Factory { get; }
    public abstract object? GetService(ServiceProvider serviceProvider, Type? genericImplementationType = null);
    public abstract ServiceAccessor CreateScopedServiceAccessor();
    public abstract void Dispose();

    public ServiceAccessor(Func<IServiceProvider, Type?, object?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        Factory = factory;
    }
}
