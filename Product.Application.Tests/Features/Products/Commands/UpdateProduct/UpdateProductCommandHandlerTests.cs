using Moq;
using Product.Application.DTOs;
using Product.Application.Features.Products.Commands.UpdateProduct;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Repositories;
using Xunit;

namespace Product.Application.Tests.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IProductCacheService> _mockCacheService;
    private readonly Mock<ILogServiceClient> _mockLogClient;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockCacheService = new Mock<IProductCacheService>();
        _mockLogClient = new Mock<ILogServiceClient>();

        _handler = new UpdateProductCommandHandler(
            _mockRepository.Object,
            _mockCacheService.Object,
            _mockLogClient.Object);
    }

    [Fact]
    public async Task Handle_WithValidProductId_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Price = 149.99m,
            Stock = 75
        };

        var existingProduct = new ProductEntity
        {
            Id = productId,
            Name = "Old Product",
            Price = 99.99m,
            Stock = 50,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal(149.99m, result.Price);
        Assert.Equal(75, result.Stock);
    }

    [Fact]
    public async Task Handle_WithInvalidProductId_ShouldReturnNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Non-Existent",
            Price = 99m,
            Stock = 10
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithInvalidProductId_ShouldSendWarningLog()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Non-Existent",
            Price = 99m,
            Stock = 10
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _mockLogClient.Verify(
            x => x.SendLogAsync(
                "Product.API",
                "WARNING",
                It.Is<string>(msg => msg.Contains("Product update failed") && msg.Contains("Product not found")),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidProductId_ShouldSendInfoLog()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated",
            Price = 50m,
            Stock = 20
        };

        var existingProduct = new ProductEntity
        {
            Id = productId,
            Name = "Old",
            Price = 25m,
            Stock = 10,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _mockLogClient.Verify(
            x => x.SendLogAsync(
                "Product.API",
                "INFO",
                It.Is<string>(msg => msg.Contains("Product updated")),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCacheOnSuccess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated",
            Price = 50m,
            Stock = 20
        };

        var existingProduct = new ProductEntity
        {
            Id = productId,
            Name = "Old",
            Price = 25m,
            Stock = 10,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockLogClient.Setup(x => x.SendLogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        _mockCacheService.Verify(
            x => x.RemoveProductsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
