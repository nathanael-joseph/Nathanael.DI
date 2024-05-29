using System;

namespace Nathanael.DI.Internal;

internal class ScopedServiceAccessor : SingletonServiceAccessor
{
    private readonly bool _isScoped = false;
    private Func<IServiceProvider, object?> _factory;

    public override Lifetime Lifetime => Lifetime.Scoped;

    private ScopedServiceAccessor(Func<IServiceProvider, object?> factory, bool isScoped) : base(factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        _factory = factory;
        _isScoped = isScoped;
    }

    public ScopedServiceAccessor(Func<IServiceProvider, object?> factory) : this(factory, false) { }

    public override ServiceAccessor CreateScopedServiceAccessor()
    {
        return new ScopedServiceAccessor(_factory, true);
    }

    public override object? GetService(ServiceProvider serviceProvider)
    {
        if(_isScoped)
        {
            return base.GetService(serviceProvider);
        }
        throw new InvalidOperationException("A scoped service cannot be requested outside of a scoped context.");
    }

    public override void Dispose()
    {
        if(_isScoped) base.Dispose();
    }
}

