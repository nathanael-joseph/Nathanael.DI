using Microsoft.AspNetCore.Mvc;
using Nathanael.DI;
using Nathanael.DI.Example.WebApp;
using Nathanael.DI.Hosting;

var builder = WebApplication.CreateBuilder(args);

ServiceProviderConfiguration diConfiguration = builder.Host.UseNathanaelDI();

diConfiguration.ProvideScoped<RandomUserRequestHandler>();
diConfiguration.ProvideSingleton<HttpClient>();

var app = builder.Build();

app.MapGet("/user", async ([FromServices] RandomUserRequestHandler handler) => await handler.HandleRequestAsync());

app.Run();


