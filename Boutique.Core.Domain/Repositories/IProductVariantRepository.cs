using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Domain.Repositories
{
    public interface IProductVariantRepository : IGenericRepository<ProductVariant>
    {
        Task<ProductVariant> GetProductVariantByIdAsync(int id);
        Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync();
        Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(int productId);

    }
}
