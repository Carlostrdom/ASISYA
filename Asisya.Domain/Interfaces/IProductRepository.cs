using Asisya.Domain.Entities;

namespace Asisya.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, int? categoryId);
    Task AddAsync(Product product);
    Task BulkInsertAsync(IEnumerable<Product> products);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
}
