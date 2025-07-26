using Microsoft.EntityFrameworkCore;
using MockECommerce.DAL.Abstract;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;

namespace MockECommerce.DAL.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryDal
{

    private readonly AppDbContext _context;
    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllWithProductsAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdWitchProductsAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}