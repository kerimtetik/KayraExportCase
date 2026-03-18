using Microsoft.Extensions.Logging;
using Product.Application.Interfaces;
using Product.Domain.Events;

namespace Product.Infrastructure.Services;

public class ProductEventPublisher : IProductEventPublisher
{
    private readonly ILogger<ProductEventPublisher> _logger;

    public ProductEventPublisher(ILogger<ProductEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishProductCreatedAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken = default)
    {
        // Basit logger implementasyonu - ileride RabbitMQ/Kafka'ya evrilebilir
        _logger.LogInformation(
            "Event published: ProductCreated | ProductId: {ProductId}, Name: {Name}, Price: {Price}, Stock: {Stock}, CreatedAtUtc: {CreatedAtUtc}",
            productCreatedEvent.ProductId,
            productCreatedEvent.Name,
            productCreatedEvent.Price,
            productCreatedEvent.Stock,
            productCreatedEvent.CreatedAtUtc);

        return Task.CompletedTask;
    }
}
