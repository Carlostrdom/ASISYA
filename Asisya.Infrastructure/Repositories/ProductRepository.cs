using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;
using Asisya.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AsisyaDbContext _context;

    public ProductRepository(AsisyaDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductID == id);

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? categoryId)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.ILike(p.ProductName, $"%{search}%"));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryID == categoryId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.ProductID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task BulkInsertAsync(IEnumerable<Product> products)
    {
        const int batchSize = 1000;
        var batch = new List<Product>(batchSize);

        foreach (var product in products)
        {
            batch.Add(product);
            if (batch.Count == batchSize)
            {
                await _context.Products.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await _context.Products.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var existing = await _context.Products.FindAsync(product.ProductID);
        if (existing is null) return false;

        _context.Entry(existing).CurrentValues.SetValues(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
