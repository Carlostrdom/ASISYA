using Asisya.Application.DTOs.Category;
using Asisya.Application.Services;
using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;
using Moq;

namespace Asisya.Tests.Unit;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();

    [Fact]
    public async Task CreateAsync_ReturnsMappedDto()
    {
        var dto = new CreateCategoryDto { CategoryName = "SERVIDORES", Description = "Servidores físicos" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
                 .ReturnsAsync((Category c) => { c.CategoryID = 1; return c; });

        var service = new CategoryService(_repoMock.Object);
        var result = await service.CreateAsync(dto);

        Assert.Equal(1, result.CategoryID);
        Assert.Equal("SERVIDORES", result.CategoryName);
        Assert.Equal("Servidores físicos", result.Description);
        Assert.Null(result.PictureBase64);
    }

    [Fact]
    public async Task CreateAsync_WithPicture_ConvertsPictureToBase64()
    {
        var pictureBytes = new byte[] { 1, 2, 3 };
        var dto = new CreateCategoryDto
        {
            CategoryName = "CLOUD",
            PictureBase64 = Convert.ToBase64String(pictureBytes)
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
                 .ReturnsAsync((Category c) => { c.CategoryID = 2; return c; });

        var service = new CategoryService(_repoMock.Object);
        var result = await service.CreateAsync(dto);

        Assert.Equal(dto.PictureBase64, result.PictureBase64);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>
        {
            new() { CategoryID = 1, CategoryName = "SERVIDORES" },
            new() { CategoryID = 2, CategoryName = "CLOUD" }
        });

        var service = new CategoryService(_repoMock.Object);
        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("SERVIDORES", result[0].CategoryName);
        Assert.Equal("CLOUD", result[1].CategoryName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var service = new CategoryService(_repoMock.Object);
        var result = await service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(new Category { CategoryID = 1, CategoryName = "SERVIDORES" });

        var service = new CategoryService(_repoMock.Object);
        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.CategoryID);
        Assert.Equal("SERVIDORES", result.CategoryName);
    }
}
