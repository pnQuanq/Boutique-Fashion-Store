using Boutique.Core.Contracts.Product;
using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IProductService
    {
        Task<ProductDto> AddProductAsync(CreateProductDto productDto);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateProductDto);
        Task<string> DeleteProductAsync(int productId);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetAllProductsForRecomendSystemAsync();
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductDto>> GetRecentProductsAsync(int count);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAndGenderAsync(int categoryId, int gender);
        Task<IEnumerable<ProductDto>> GetProductsByGenderAsync(int gender);
        Task<List<ProductDto>> SearchProductsByNameAsync(string searchString);

    }
}
