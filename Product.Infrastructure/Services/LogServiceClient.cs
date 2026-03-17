using System.Net.Http.Json;
using Product.Application.Interfaces;

namespace Product.Infrastructure.Services;

public class LogServiceClient : ILogServiceClient
{
    private readonly HttpClient _httpClient;

    public LogServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendLogAsync(string serviceName, string level, string message, string? exception = null, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            serviceName,
            level,
            message,
            exception
        };

        await _httpClient.PostAsJsonAsync("/api/logs", payload, cancellationToken);
    }
}