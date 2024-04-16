
using Nathanael.DI;

var builder = WebApplication.CreateBuilder(args);

//IServiceProviderFactory<ServiceProviderBuilder> scf = 
//builder.Host.UseServiceProviderFactory()

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
