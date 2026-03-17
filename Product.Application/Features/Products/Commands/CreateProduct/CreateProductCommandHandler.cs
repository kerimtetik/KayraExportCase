using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Repositories;

namespace Product.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCacheService _productCacheService;
    private readonly ILogServiceClient _logServiceClient;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IProductCacheService productCacheService,
        ILogServiceClient logServiceClient)

    {
        _productRepository = productRepository;
        _productCacheService = productCacheService;
        _logServiceClient = logServiceClient;

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
        await _logServiceClient.SendLogAsync(
            "Product.API",
            "INFO",
            $"Product created. Id: {product.Id}, Name: {product.Name}",
            null,
            cancellationToken);

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