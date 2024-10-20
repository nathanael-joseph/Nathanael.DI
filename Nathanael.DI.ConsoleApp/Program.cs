using Nathanael.DI;

internal class Program
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

            config.ProvideTransient<RandomUserRequestHandler>();

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

            var handler = sp.GetRequiredService<RandomUserRequestHandler>();
            await handler.HandleRequestAsync();
        }

        

    }
}

public class RandomUserRequestHandler
{
    private readonly Uri _getRandomUserUri = new("https://randomuser.me/api/");
    private readonly HttpClient _httpClient;
    private readonly IMessageLogger _logger;

    public RandomUserRequestHandler(HttpClient httpClient,
                                    IMessageLogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task HandleRequestAsync()
    {
        _logger.Log($"Sending GET requesdt to {_getRandomUserUri}");
        var response = await _httpClient.GetAsync(_getRandomUserUri);
        _logger.Log($"GET requesdt to {_getRandomUserUri} return status code: {response.StatusCode}");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}
