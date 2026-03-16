using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Repositories;

namespace Product.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCacheService _productCacheService;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        IProductCacheService productCacheService,
        ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _productCacheService = productCacheService;
        _logger = logger;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        var cachedProducts = await _productCacheService.GetProductsAsync(cancellationToken);

        if (cachedProducts is not null)
        {
            _logger.LogInformation("Products returned from REDIS cache.");
            return cachedProducts;
        }

        _logger.LogInformation("Products returned from DATABASE.");

        var products = await _productRepository.GetAllAsync(cancellationToken);

        var result = products.Select(product => new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        }).ToList();

        await _productCacheService.SetProductsAsync(result, cancellationToken);

        return result;
    }
}