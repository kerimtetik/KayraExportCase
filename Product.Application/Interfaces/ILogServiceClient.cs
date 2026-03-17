namespace Product.Application.Interfaces;

public interface ILogServiceClient
{
    Task SendLogAsync(string serviceName, string level, string message, string? exception = null, CancellationToken cancellationToken = default);
}