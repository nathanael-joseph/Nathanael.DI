using System;

namespace Nathanael.DI.Internal;

internal class TransientServiceAccessor : ServiceAccessor
{
    private readonly Func<IServiceProvider, object?> _factory;

    public TransientServiceAccessor(Func<IServiceProvider, object?> factory)
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
        return _factory(serviceProvider);
    }

    public override void Dispose()
    {
        // no op.
    }
}

