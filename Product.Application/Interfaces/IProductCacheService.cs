using Product.Application.DTOs;

namespace Product.Application.Interfaces;

public interface IProductCacheService
{
    Task<List<ProductResponseDto>?> GetProductsAsync(CancellationToken cancellationToken = default);
    Task SetProductsAsync(List<ProductResponseDto> products, CancellationToken cancellationToken = default);
    Task RemoveProductsAsync(CancellationToken cancellationToken = default);
}