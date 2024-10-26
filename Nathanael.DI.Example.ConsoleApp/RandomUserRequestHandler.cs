namespace Nathanael.DI.Example.ConsoleApp;

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
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{Environment.NewLine}{result}");
    }
}
