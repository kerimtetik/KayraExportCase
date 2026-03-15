using Product.Application.DTOs;
using Product.Domain.Repositories;

namespace Product.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponseDto?> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Name = command.Name;
        product.Price = command.Price;
        product.Stock = command.Stock;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);

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