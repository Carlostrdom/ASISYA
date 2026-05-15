using Asisya.Application.DTOs.Category;

namespace Asisya.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
}
