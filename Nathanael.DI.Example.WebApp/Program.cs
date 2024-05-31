using Nathanael.DI.Hosting;

var builder = WebApplication.CreateBuilder(args);

ServiceProviderFactory spf = new();


builder.Host.UseServiceProviderFactory(spf);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


