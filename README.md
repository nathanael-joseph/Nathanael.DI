# Nathanael.DI

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