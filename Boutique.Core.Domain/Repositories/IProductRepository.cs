using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Domain.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<Product> GetProductWithCategoryAsync(int Id);
    }
}
