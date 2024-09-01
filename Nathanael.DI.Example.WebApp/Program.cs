using Nathanael.DI.Hosting;

var builder = WebApplication.CreateBuilder(args);

var diconfig = builder.Host.UseNathanaelDI();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


