using Microsoft.Extensions.Logging;
using Moq;
using Product.Domain.Events;
using Product.Infrastructure.Services;
using Xunit;

namespace Product.Application.Tests.Services;

public class ProductEventPublisherTests
{
    private readonly Mock<ILogger<ProductEventPublisher>> _mockLogger;
    private readonly ProductEventPublisher _publisher;

    public ProductEventPublisherTests()
    {
        _mockLogger = new Mock<ILogger<ProductEventPublisher>>();
        _publisher = new ProductEventPublisher(_mockLogger.Object);
    }

    [Fact]
    public async Task PublishProductCreatedAsync_ShouldLogEvent()
    {
        // Arrange
        var @event = new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            Name = "Test Product",
            Price = 99.99m,
            Stock = 50,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Act
        await _publisher.PublishProductCreatedAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ProductCreated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishProductCreatedAsync_ShouldIncludeEventDetails()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var @event = new ProductCreatedEvent
        {
            ProductId = productId,
            Name = "Detailed Product",
            Price = 149.99m,
            Stock = 100,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Act
        await _publisher.PublishProductCreatedAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Detailed Product") &&
                    v.ToString()!.Contains("149.99") &&
                    v.ToString()!.Contains("100")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishProductCreatedAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var @event = new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            Name = "Test",
            Price = 50m,
            Stock = 10,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Act & Assert (no exception thrown)
        await _publisher.PublishProductCreatedAsync(@event);

        _mockLogger.Verify(
            x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishProductCreatedAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var @event = new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            Name = "Cancel Test",
            Price = 75m,
            Stock = 25,
            CreatedAtUtc = DateTime.UtcNow
        };
        var cancellationToken = CancellationToken.None;

        // Act
        await _publisher.PublishProductCreatedAsync(@event, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
