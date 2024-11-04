# Nathanael.DI

> Nathanael.DI `latest: 1.0.0`

> Nathanael.DI.Hosting `latest: 1.0.0`

A custom dependency injection library developed for educational purposes and professional use.

## Installing

If you intend on using this DI library within a hosted application, you will need to install Nathanael.DI.Hosting

```bash
dotnet add [<PROJECT>] package Nathanael.DI.Hosting 
```

However if you are not using Nathanael.DI within the context of a hosted application, you can simply install the core DI library package

```bash
dotnet add [<PROJECT>] package Nathanael.DI
```

## Building the Service Provider

```csharp
using Nathanael.DI;
...
// configure and build service provider...
var builder = new ServiceProviderBuilder();

builder.Configure(config =>
{
    // configure services here
});

ServiceProvider sp = builder.Build();
```

When working within a hosted application framework, the host builder is responsible for building the application's service provider. We can configure the host builder to use our service provider implementation like so:

```csharp
using Nathanael.DI;
using Nathanael.DI.Hosting;
...
var builder = WebApplication.CreateBuilder(args);
ServiceProviderConfiguration diConfiguration = builder.Host.UseNathanaelDI();
// configure services here
var app = builder.Build();
...
app.Run();
```

## Configuring Services

The `ServiceConfiguration` class defines a service which the service provider should provide. It defines which services it will be used to resolve, how it should be instanciated, and when it should be instanciated. These behaviours are defined by the configuration's `RegisteredServiceTypes`, `FactoryMethod`, and `Lifetime` properties.

### Lifetime

Our service provider supports configurging three service lifetimes.

#### Transient

A new instance should be generated whenever the service is resolved.

```csharp
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.ProvideTransient<ServiceA>();
});
```

#### Singleton

The same instance should be returned whenever the service is resolved.

```csharp
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.ProvideSingeton<ServiceA>();
});
```

If the singleton's service is already instanced at configuration time, the service provider can be configured to provide the instance like so:

```csharp
ServiceA serviceA = ...;
...
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.Provide(serviceA);
});
```

#### Scoped

The same instance should be returned every time the service is resolved within the same scope. A nerw instance will be generated for each scope when the service is first resolved.

```csharp
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.ProvideScoped<ServiceA>();
});
```

### Instanciation

By default, a service will be instanciated by calling the implementation type's public constructor with the most resolvable arguments. You can mark a specifica public constructor to use for instanciatyion using the `DIConstructorAttribute`.

```csharp
public class ServiceA
{
    public ServiceA(DependencyA depa)
    { ... }

    [DIConstructor] // use this constructor for service instanciation
    public ServiceA(DependencyB depb)
    { ... }
}
```

A custom factory delegate can also be configured like so:

```csharp
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.ProvideTransient<ServiceA>()
            .FromFactory(sp => new ServiceA() /* example factory method */);
});
```

Factory delegates accept an `IServiceProvider` argument which can be used to resolve dependencies during instanciation.

### Resolvable Services

By default, a service will only be used to resolve it's implementation type. The service types for which the implemntation should be used to resolve can be specified using the `WhenResolving<TDep>()` method.

```csharp
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.ProvideTransient<ServiceA>()
            .WhenResolving<IServiceA>();
});
```

A single service can be used to resolve multiple service types, providing it implements them or inherits from them:

```csharp
var builder = new ServiceProviderBuilder().Configure(provider =>
{
    provider.ProvideTransient<ServiceAB>()
            .WhenResolving<IServiceA>()
            .WhenResolving<IServiceB>()
            .WhenResolving<IServiceAB>();
});
```

## Using the ServiceProvider

Once we have configured and built our service provider, we can use to resolve our application's service.

```csharp
using Nathanael.DI;
...
var builder = new ServiceProviderBuilder().Configure(config =>
{
    ...
});
ServiceProvider sp = builder.Build();

// for a service whioch may or may not be resolvable
ServiceA? serviceA = sp.GetService<ServiceA>();

// for a service which must be resolvable
ServiceB serviceB = sp.GetRequiredService<ServiceB>();
```

