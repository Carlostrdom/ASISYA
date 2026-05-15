using Asisya.Application.DTOs.Common;
using Asisya.Application.DTOs.Product;
using Asisya.Application.Interfaces;
using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Asisya.Application.Services;

public class ProductService : IProductService
{
    private static readonly string[] Adjectives = ["Ultra", "Pro", "Smart", "Cloud", "Turbo", "Advanced", "Next-Gen", "Enterprise", "Nano", "Hyper"];
    private static readonly string[] Types = ["Server", "Switch", "Router", "Firewall", "Storage", "CPU", "GPU", "RAM", "SSD", "NIC"];
    private const string CacheTokenKey = "products_cache_token";

    private readonly IProductRepository _repository;
    private readonly IMemoryCache _cache;

    public ProductService(IProductRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize, string? search, int? categoryId)
    {
        var cacheKey = $"products_p{page}_s{pageSize}_q{search}_c{categoryId}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            entry.AddExpirationToken(GetCacheToken());

            var (items, total) = await _repository.GetPagedAsync(page, pageSize, search, categoryId);
            return new PagedResultDto<ProductDto>
            {
                Items = items.Select(ToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }) ?? new PagedResultDto<ProductDto>();
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : ToDetailDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            ProductName = dto.ProductName,
            SupplierID = dto.SupplierID,
            CategoryID = dto.CategoryID,
            QuantityPerUnit = dto.QuantityPerUnit,
            UnitPrice = dto.UnitPrice,
            UnitsInStock = dto.UnitsInStock,
            UnitsOnOrder = dto.UnitsOnOrder,
            ReorderLevel = dto.ReorderLevel,
            Discontinued = dto.Discontinued
        };

        await _repository.AddAsync(product);
        InvalidateCache();
        return ToDto(product);
    }

    public async Task BulkCreateAsync(BulkCreateProductDto dto)
    {
        var rng = new Random();
        var products = Enumerable.Range(0, dto.Count).Select(_ => new Product
        {
            ProductName = $"{Adjectives[rng.Next(Adjectives.Length)]} {Types[rng.Next(Types.Length)]} {rng.Next(1000, 99999)}",
            CategoryID = dto.CategoryID,
            UnitPrice = Math.Round((decimal)(rng.NextDouble() * 9999 + 0.99), 2),
            UnitsInStock = (short)rng.Next(0, 500),
            UnitsOnOrder = (short)rng.Next(0, 100),
            ReorderLevel = (short)rng.Next(0, 50),
            Discontinued = false
        });

        await _repository.BulkInsertAsync(products);
        InvalidateCache();
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return false;

        if (dto.ProductName is not null) existing.ProductName = dto.ProductName;
        if (dto.SupplierID.HasValue) existing.SupplierID = dto.SupplierID;
        if (dto.CategoryID.HasValue) existing.CategoryID = dto.CategoryID;
        if (dto.QuantityPerUnit is not null) existing.QuantityPerUnit = dto.QuantityPerUnit;
        if (dto.UnitPrice.HasValue) existing.UnitPrice = dto.UnitPrice.Value;
        if (dto.UnitsInStock.HasValue) existing.UnitsInStock = dto.UnitsInStock.Value;
        if (dto.UnitsOnOrder.HasValue) existing.UnitsOnOrder = dto.UnitsOnOrder.Value;
        if (dto.ReorderLevel.HasValue) existing.ReorderLevel = dto.ReorderLevel.Value;
        if (dto.Discontinued.HasValue) existing.Discontinued = dto.Discontinued.Value;

        var result = await _repository.UpdateAsync(existing);
        if (result) InvalidateCache();
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result) InvalidateCache();
        return result;
    }

    private Microsoft.Extensions.Primitives.IChangeToken GetCacheToken()
    {
        if (!_cache.TryGetValue(CacheTokenKey, out CancellationTokenSource? cts) || cts!.IsCancellationRequested)
        {
            cts = new CancellationTokenSource();
            _cache.Set(CacheTokenKey, cts, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
        }
        return new Microsoft.Extensions.Primitives.CancellationChangeToken(cts.Token);
    }

    private void InvalidateCache()
    {
        if (_cache.TryGetValue(CacheTokenKey, out CancellationTokenSource? cts))
            cts?.Cancel();
    }

    private static ProductDto ToDto(Product p) => new()
    {
        ProductID = p.ProductID,
        ProductName = p.ProductName,
        CategoryID = p.CategoryID,
        CategoryName = p.Category?.CategoryName,
        UnitPrice = p.UnitPrice,
        UnitsInStock = p.UnitsInStock,
        Discontinued = p.Discontinued
    };

    private static ProductDetailDto ToDetailDto(Product p) => new()
    {
        ProductID = p.ProductID,
        ProductName = p.ProductName,
        CategoryID = p.CategoryID,
        CategoryName = p.Category?.CategoryName,
        CategoryPictureBase64 = p.Category?.Picture is not null
            ? Convert.ToBase64String(p.Category.Picture)
            : null,
        SupplierID = p.SupplierID,
        SupplierName = p.Supplier?.CompanyName,
        QuantityPerUnit = p.QuantityPerUnit,
        UnitPrice = p.UnitPrice,
        UnitsInStock = p.UnitsInStock,
        UnitsOnOrder = p.UnitsOnOrder,
        ReorderLevel = p.ReorderLevel,
        Discontinued = p.Discontinued
    };
}
