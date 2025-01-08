using Boutique.Core.Contracts.Category;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Contracts.ProductVariant;

namespace Boutique.Web.ViewModel.ProductVariant
{
    public class ProductVariantViewModel
    {
        public IEnumerable<ProductVariantDto> ProductVariants { get; set; } = new List<ProductVariantDto>();
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }
}
