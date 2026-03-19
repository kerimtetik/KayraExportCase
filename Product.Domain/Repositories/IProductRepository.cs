using Product.Domain.Entities;

namespace Product.Domain.Repositories;

public interface IProductRepository
{
    Task AddAsync(ProductEntity product, CancellationToken cancellationToken = default);
    Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ProductEntity>> GetPageAsync(DateTime? cursorCreatedAtUtc, Guid? cursorId, int limit, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default);
}
