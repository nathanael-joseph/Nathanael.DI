using System;

namespace Nathanael.DI.Internal;

internal abstract class ServiceAccessor : IDisposable
{
    public abstract Lifetime Lifetime { get; }
    public abstract object? GetService(ServiceProvider serviceProvider);
    public abstract ServiceAccessor CreateScopedServiceAccessor();
    public abstract void Dispose();
}
