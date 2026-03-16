using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.Infrastructure.Caching;

public class ProductCacheService : IProductCacheService
{
    private const string ProductsCacheKey = "products:list";
    private readonly IDistributedCache _distributedCache;

    public ProductCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<List<ProductResponseDto>?> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(ProductsCacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(cachedData))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<ProductResponseDto>>(cachedData);
    }

    public async Task SetProductsAsync(List<ProductResponseDto> products, CancellationToken cancellationToken = default)
    {
        var serializedData = JsonSerializer.Serialize(products);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _distributedCache.SetStringAsync(ProductsCacheKey, serializedData, options, cancellationToken);
    }

    public async Task RemoveProductsAsync(CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(ProductsCacheKey, cancellationToken);
    }
}