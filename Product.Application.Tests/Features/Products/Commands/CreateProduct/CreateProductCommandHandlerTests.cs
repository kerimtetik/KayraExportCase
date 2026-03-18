using Moq;
using Product.Application.DTOs;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Interfaces;
using Product.Domain.Events;
using Product.Domain.Repositories;
using Xunit;

namespace Product.Application.Tests.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IProductCacheService> _mockCacheService;
    private readonly Mock<ILogServiceClient> _mockLogClient;
    private readonly Mock<IProductEventPublisher> _mockEventPublisher;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockCacheService = new Mock<IProductCacheService>();
        _mockLogClient = new Mock<ILogServiceClient>();
        _mockEventPublisher = new Mock<IProductEventPublisher>();

        _handler = new CreateProductCommandHandler(
            _mockRepository.Object,
            _mockCacheService.Object,
            _mockLogClient.Object,
            _mockEventPublisher.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Price = 99.99m,
            Stock = 50
        };

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Product.Domain.Entities.ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventPublisher.Setup(x => x.PublishProductCreatedAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(50, result.Stock);
        Assert.NotEqual(Guid.Empty, result.Id);

        // Verify repository was called
        _mockRepository.Verify(
            x => x.AddAsync(It.IsAny<Product.Domain.Entities.ProductEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test",
            Price = 50m,
            Stock = 10
        };

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Product.Domain.Entities.ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventPublisher.Setup(x => x.PublishProductCreatedAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _mockCacheService.Verify(
            x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendInfoLog()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Price = 99.99m,
            Stock = 50
        };

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Product.Domain.Entities.ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventPublisher.Setup(x => x.PublishProductCreatedAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _mockLogClient.Verify(
            x => x.SendLogAsync(
                "Product.API",
                "INFO",
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishProductCreatedEvent()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Event Test Product",
            Price = 75m,
            Stock = 25
        };

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Product.Domain.Entities.ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventPublisher.Setup(x => x.PublishProductCreatedAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _mockEventPublisher.Verify(
            x => x.PublishProductCreatedAsync(
                It.Is<ProductCreatedEvent>(e =>
                    e.Name == "Event Test Product" &&
                    e.Price == 75m &&
                    e.Stock == 25),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
