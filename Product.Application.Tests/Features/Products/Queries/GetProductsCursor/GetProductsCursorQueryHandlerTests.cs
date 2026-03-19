using Microsoft.Extensions.Logging;
using Moq;
using Product.Application.Features.Products.Queries.GetProductsCursor;
using Product.Domain.Entities;
using Product.Domain.Repositories;
using Xunit;

namespace Product.Application.Tests.Features.Products.Queries.GetProductsCursor;

public class GetProductsCursorQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<GetProductsCursorQueryHandler>> _mockLogger;
    private readonly GetProductsCursorQueryHandler _handler;

    public GetProductsCursorQueryHandlerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<GetProductsCursorQueryHandler>>();
        _handler = new GetProductsCursorQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_FirstPage_ReturnsItemsAndNextCursor()
    {
        var createdAt = DateTime.UtcNow;
        var products = new List<ProductEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "P1", Price = 10, Stock = 1, CreatedAtUtc = createdAt },
            new() { Id = Guid.NewGuid(), Name = "P2", Price = 20, Stock = 2, CreatedAtUtc = createdAt.AddMinutes(-1) },
            new() { Id = Guid.NewGuid(), Name = "P3", Price = 30, Stock = 3, CreatedAtUtc = createdAt.AddMinutes(-2) }
        };

        _mockRepository
            .Setup(x => x.GetPageAsync(null, null, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.Handle(new GetProductsCursorQuery { Limit = 2 });

        Assert.Equal(2, result.Items.Count);
        Assert.True(result.HasNext);
        Assert.Equal(2, result.Limit);
        Assert.False(string.IsNullOrWhiteSpace(result.NextCursor));
    }

    [Fact]
    public async Task Handle_InvalidCursor_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(new GetProductsCursorQuery { Cursor = "not-a-valid-cursor", Limit = 10 }));
    }

    [Fact]
    public async Task Handle_UsesDefaultLimit_WhenLimitIsZero()
    {
        _mockRepository
            .Setup(x => x.GetPageAsync(null, null, GetProductsCursorQueryHandler.DefaultLimit + 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetProductsCursorQuery { Limit = 0 });

        Assert.Equal(GetProductsCursorQueryHandler.DefaultLimit, result.Limit);
    }

    [Fact]
    public async Task Handle_UsesMaxLimit_WhenLimitExceedsMax()
    {
        _mockRepository
            .Setup(x => x.GetPageAsync(null, null, GetProductsCursorQueryHandler.MaxLimit + 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetProductsCursorQuery { Limit = 999 });

        Assert.Equal(GetProductsCursorQueryHandler.MaxLimit, result.Limit);
    }
}
