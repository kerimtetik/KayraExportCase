using Product.Domain.Events;

namespace Product.Application.Interfaces;

public interface IProductEventPublisher
{
    Task PublishProductCreatedAsync(ProductCreatedEvent productCreatedEvent, CancellationToken cancellationToken = default);
}
