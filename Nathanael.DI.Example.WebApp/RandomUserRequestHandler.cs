namespace Nathanael.DI.Example.WebApp;

public class RandomUserRequestHandler
{
    private readonly Uri _getRandomUserUri = new("https://randomuser.me/api/");
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public RandomUserRequestHandler(HttpClient httpClient,
                                    ILogger<RandomUserRequestHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> HandleRequestAsync()
    {
        _logger.LogInformation("Sending GET requesdt to {Uri}", _getRandomUserUri);
        var response = await _httpClient.GetAsync(_getRandomUserUri);
        _logger.LogInformation("GET requesdt to {Uri} return status code: {StatusCode}", _getRandomUserUri, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}
