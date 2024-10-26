namespace Nathanael.DI.Example.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        // configure and build service provider...
        var builder = new ServiceProviderBuilder();

        builder.Configure(config =>
        {
            config.ProvideSingleton<FileLogger>()
                  .WhenResolving<IMessageLogger>();

            config.ProvideSingleton<HttpClient>();

            config.ProvideScoped<RandomUserRequestHandler>();
        });

        var sp = builder.Build();

        // use service provider in console application:
        var logger = sp.GetRequiredService<IMessageLogger>();
        logger.Log("Starting console application");

        Console.WriteLine("Enter any key to generate a random user. Enter Q to exit");

        while(true)
        {
            var key = Console.ReadKey();

            if (key.KeyChar == '\0') continue;
            
            if (key.KeyChar == 'q' || key.KeyChar == 'Q')
            {
                logger.Log("Shutting down console application.");
                sp.Dispose();
                Environment.Exit(0);
            }

            using (var scopedServiceProvider = sp.CreateScopedServiceProvider())
            {
                var handler = scopedServiceProvider.GetRequiredService<RandomUserRequestHandler>();
                await handler.HandleRequestAsync();
            }   
        }
    }
}
