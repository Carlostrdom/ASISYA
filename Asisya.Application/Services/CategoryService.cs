using Asisya.Application.DTOs.Category;
using Asisya.Application.Interfaces;
using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;

namespace Asisya.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            Picture = dto.PictureBase64 is not null
                ? Convert.FromBase64String(dto.PictureBase64)
                : null
        };

        var created = await _repository.AddAsync(category);
        return ToDto(created);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(ToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : ToDto(category);
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        CategoryID = c.CategoryID,
        CategoryName = c.CategoryName,
        Description = c.Description,
        PictureBase64 = c.Picture is not null ? Convert.ToBase64String(c.Picture) : null
    };
}
