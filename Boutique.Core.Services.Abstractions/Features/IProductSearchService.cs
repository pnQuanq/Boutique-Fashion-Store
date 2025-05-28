
using Boutique.Core.Contracts.Product;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IProductSearchService
    {
        Task InitializeAutoTFSWeightsAsync();
        Task<List<ProductDto>> SearchProductsByNameAsync(string searchString);
    }
}
