using Asisya.Application.DTOs.Common;
using Asisya.Application.DTOs.Product;

namespace Asisya.Application.Interfaces;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize, string? search, int? categoryId);
    Task<ProductDetailDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task BulkCreateAsync(BulkCreateProductDto dto);
    Task<bool> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
}
