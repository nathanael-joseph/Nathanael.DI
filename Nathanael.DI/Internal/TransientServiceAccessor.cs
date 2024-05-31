using System;

namespace Nathanael.DI.Internal;

internal class TransientServiceAccessor : ServiceAccessor
{
    private readonly Func<IServiceProvider, Type?, object?> _factory;

    public override Lifetime Lifetime => Lifetime.Transient;

    public TransientServiceAccessor(Func<IServiceProvider, Type?, object?> factory)
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
        return _factory(serviceProvider, genericImplementationType);
    }

    public override void Dispose()
    {
        // no op.
    }
}

