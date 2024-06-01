using Nathanael.DI.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNathanaelDI();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


