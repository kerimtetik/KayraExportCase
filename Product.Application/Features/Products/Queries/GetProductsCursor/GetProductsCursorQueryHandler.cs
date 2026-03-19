using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Domain.Repositories;

namespace Product.Application.Features.Products.Queries.GetProductsCursor;

public class GetProductsCursorQueryHandler
{
    public const int DefaultLimit = 10;
    public const int MaxLimit = 50;

    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductsCursorQueryHandler> _logger;

    public GetProductsCursorQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductsCursorQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductCursorPageResponseDto> Handle(GetProductsCursorQuery query, CancellationToken cancellationToken = default)
    {
        var limit = NormalizeLimit(query.Limit);

        DateTime? cursorCreatedAtUtc = null;
        Guid? cursorId = null;

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            var cursor = ProductCursorHelper.Decode(query.Cursor);
            cursorCreatedAtUtc = cursor.CreatedAtUtc;
            cursorId = cursor.Id;
        }

        var products = await _productRepository.GetPageAsync(
            cursorCreatedAtUtc,
            cursorId,
            limit + 1,
            cancellationToken);

        var hasNext = products.Count > limit;
        var pageItems = products.Take(limit).Select(product => new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        }).ToList();

        string? nextCursor = null;
        if (hasNext && pageItems.Count > 0)
        {
            var lastItem = pageItems[^1];
            nextCursor = ProductCursorHelper.Encode(new ProductCursorModel
            {
                CreatedAtUtc = lastItem.CreatedAtUtc,
                Id = lastItem.Id
            });
        }

        _logger.LogInformation("Products returned from DATABASE with cursor pagination. Limit: {Limit}, HasNext: {HasNext}", limit, hasNext);

        return new ProductCursorPageResponseDto
        {
            Items = pageItems,
            NextCursor = nextCursor,
            HasNext = hasNext,
            Limit = limit
        };
    }

    private static int NormalizeLimit(int limit)
    {
        if (limit <= 0)
        {
            return DefaultLimit;
        }

        return Math.Min(limit, MaxLimit);
    }
}
