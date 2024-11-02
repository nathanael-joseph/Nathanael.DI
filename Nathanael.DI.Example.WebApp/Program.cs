using Nathanael.DI;
using Nathanael.DI.Hosting;
// ...
var builder = WebApplication.CreateBuilder(args);

ServiceProviderConfiguration diConfiguration = builder.Host.UseNathanaelDI();

// configure services here

var app = builder.Build();

// ...
app.MapGet("/", () => "Hello World!");

app.Run();


