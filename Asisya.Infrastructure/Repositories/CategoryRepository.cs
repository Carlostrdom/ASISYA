using Asisya.Domain.Entities;
using Asisya.Domain.Interfaces;
using Asisya.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AsisyaDbContext _context;

    public CategoryRepository(AsisyaDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id) =>
        await _context.Categories.FindAsync(id);

    public async Task<IEnumerable<Category>> GetAllAsync() =>
        await _context.Categories.AsNoTracking().ToListAsync();

    public async Task<Category> AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }
}
