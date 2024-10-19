using System;

namespace Nathanael.DI.Internal;

internal class TransientServiceAccessor : ServiceAccessor
{
    public TransientServiceAccessor(Func<IServiceProvider, Type?, object?> factory) : base(factory)
    {
    }   

    public override ServiceAccessor CreateScopedServiceAccessor()
    {
        return this;
    }

    public override object? GetService(ServiceProvider serviceProvider, Type? genericImplementationType = null)
    {
        return Factory(serviceProvider, genericImplementationType);
    }

    public override void Dispose()
    {
        // no op.
    }
}

