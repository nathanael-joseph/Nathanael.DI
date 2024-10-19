using System;

namespace Nathanael.DI.Internal;

internal class ScopedServiceAccessor : SingletonServiceAccessor
{
    private readonly bool _isScoped = false;

    private ScopedServiceAccessor(Func<IServiceProvider, Type?, object?> factory, bool isScoped) : base(factory)
    {
        _isScoped = isScoped;
    }

    public ScopedServiceAccessor(Func<IServiceProvider, Type?, object?> factory) : this(factory, false) { }

    public override ServiceAccessor CreateScopedServiceAccessor()
    {
        return new ScopedServiceAccessor(Factory, true);
    }

    public override object? GetService(ServiceProvider serviceProvider, Type? genericImplementationType = null)
    {
        if(_isScoped)
        {
            return base.GetService(serviceProvider, genericImplementationType);
        }
        throw new InvalidOperationException("A scoped service cannot be requested outside of a scoped context.");
    }

    public override void Dispose()
    {
        if(_isScoped) base.Dispose();
    }
}

