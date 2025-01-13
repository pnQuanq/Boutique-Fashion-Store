using Boutique.Core.Domain.Entities;
using Boutique.Core.Domain.Repositories;
using Boutique.Infrastructure.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Boutique.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
        }
        public async Task<Product> GetProductWithCategoryAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }
        public async Task<IEnumerable<Product>> GetGetRecentProductsAsync(int count)
        {
            return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .OrderByDescending(p => p.DateCreated)
            .Take(count)
            .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAndGenderAsync(int categoryId, int gender)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId && p.Category.Gender == gender)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<List<Product>> SearchProductsByNameAsync(string searchString)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{searchString.ToLower()}%"))
                .ToListAsync();
        }
    }
}
