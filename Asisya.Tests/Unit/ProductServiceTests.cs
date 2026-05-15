using Asisya.Application.DTOs.Product;
using Asisya.Application.Services;
using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Asisya.Tests.Unit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly IMemoryCache _cache;

    public ProductServiceTests()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        _cache = services.BuildServiceProvider().GetRequiredService<IMemoryCache>();
    }

    private ProductService CreateService() => new(_repoMock.Object, _cache);

    [Fact]
    public async Task CreateAsync_ReturnsMappedDto()
    {
        var dto = new CreateProductDto
        {
            ProductName = "Ultra Server 1234",
            CategoryID = 1,
            UnitPrice = 999.99m,
            UnitsInStock = 10
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                 .Callback<Product>(p => p.ProductID = 42)
                 .Returns(Task.CompletedTask);

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal(42, result.ProductID);
        Assert.Equal("Ultra Server 1234", result.ProductName);
        Assert.Equal(999.99m, result.UnitPrice);
    }

    [Fact]
    public async Task BulkCreateAsync_CallsRepositoryWithCorrectCount()
    {
        var dto = new BulkCreateProductDto { Count = 5000, CategoryID = 1 };
        IEnumerable<Product>? captured = null;

        _repoMock.Setup(r => r.BulkInsertAsync(It.IsAny<IEnumerable<Product>>()))
                 .Callback<IEnumerable<Product>>(p => captured = p.ToList())
                 .Returns(Task.CompletedTask);

        await CreateService().BulkCreateAsync(dto);

        _repoMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<Product>>()), Times.Once);
        Assert.NotNull(captured);
        Assert.Equal(5000, captured!.Count());
        Assert.All(captured, p => Assert.Equal(1, p.CategoryID));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectMetadata()
    {
        _repoMock.Setup(r => r.GetPagedAsync(2, 10, null, null))
                 .ReturnsAsync((Enumerable.Range(1, 10).Select(i => new Product { ProductID = i }), 95));

        var result = await CreateService().GetPagedAsync(2, 10, null, null);

        Assert.Equal(95, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.TotalPages);
        Assert.Equal(10, result.Items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_UsesCache_OnSecondCall()
    {
        _repoMock.Setup(r => r.GetPagedAsync(1, 10, null, null))
                 .ReturnsAsync((new List<Product>(), 0));

        var service = CreateService();
        await service.GetPagedAsync(1, 10, null, null);
        await service.GetPagedAsync(1, 10, null, null);

        _repoMock.Verify(r => r.GetPagedAsync(1, 10, null, null), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenProductNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await CreateService().UpdateAsync(99, new UpdateProductDto { ProductName = "Nuevo" });

        Assert.False(result);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields_WhenProductExists()
    {
        var existing = new Product { ProductID = 1, ProductName = "Viejo", UnitPrice = 10m };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);

        var result = await CreateService().UpdateAsync(1, new UpdateProductDto { ProductName = "Nuevo", UnitPrice = 99m });

        Assert.True(result);
        Assert.Equal("Nuevo", existing.ProductName);
        Assert.Equal(99m, existing.UnitPrice);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProductNotFound()
    {
        _repoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

        var result = await CreateService().DeleteAsync(99);

        Assert.False(result);
    }
}
