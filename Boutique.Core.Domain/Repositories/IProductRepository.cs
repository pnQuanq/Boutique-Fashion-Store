using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Domain.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<Product> GetProductWithCategoryAsync(int Id);
        Task<IEnumerable<Product>> GetGetRecentProductsAsync(int count);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsByCategoryAndGenderAsync(int categoryId, int gender);
        Task<List<Product>> SearchProductsByNameAsync(string searchString);
    }
}
