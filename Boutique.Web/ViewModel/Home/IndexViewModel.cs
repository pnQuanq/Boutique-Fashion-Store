using Boutique.Core.Contracts.Product;

namespace Boutique.Web.ViewModel.Home
{
    public class IndexViewModel
    {
        public IEnumerable<ProductDto> RecentProducts { get; set; } = new List<ProductDto>();
        public ProductRecommendationDto RecommendProducts { get; set; }
    }
}
