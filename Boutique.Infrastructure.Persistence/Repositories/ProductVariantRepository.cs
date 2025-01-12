using Boutique.Core.Domain.Entities;
using Boutique.Core.Domain.Repositories;
using Boutique.Infrastructure.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Boutique.Infrastructure.Persistence.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductVariantRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<ProductVariant> GetProductVariantByIdAsync(int id)
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product)
				    .ThenInclude(pv => pv.Images)
				.Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .FirstOrDefaultAsync(pv => pv.ProductVariantId == id);
        }
        public async Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync()
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product)
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(pv => pv.ProductId == productId)
                .Include(pv => pv.Product)
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .ToListAsync();
        }
    }
}
