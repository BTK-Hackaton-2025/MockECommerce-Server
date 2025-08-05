using Microsoft.EntityFrameworkCore;
using MockECommerce.DAL.Abstract;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;
using MockECommerce.DAL.Enums;

namespace MockECommerce.DAL.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductDal
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Product>> GetAllWithDetailsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Product>> GetBySellerIdAsync(Guid sellerId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByStatusAsync(ProductStatus status)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.Status == ProductStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .Where(p => p.IsActive && 
                       p.Status == ProductStatus.Active &&
                       (p.Title.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm) ||
                        p.Category.CategoryName.Contains(searchTerm)))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ThenInclude(s => s.User)
            .Include(p => p.Images)
            .Where(p => p.IsActive && 
                       p.Status == ProductStatus.Active &&
                       p.Price >= minPrice && 
                       p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }
}
