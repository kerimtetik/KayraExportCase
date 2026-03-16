using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Repositories;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCacheService _productCacheService;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IProductCacheService productCacheService)
    {
        _productRepository = productRepository;
        _productCacheService = productCacheService;
    }

    public async Task<ProductResponseDto> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Price = command.Price,
            Stock = command.Stock,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _productCacheService.RemoveProductsAsync(cancellationToken);

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        };
    }
}