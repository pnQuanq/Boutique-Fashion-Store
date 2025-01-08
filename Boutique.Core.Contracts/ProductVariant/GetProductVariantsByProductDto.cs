using Boutique.Core.Contracts.Product;

namespace Boutique.Core.Contracts.ProductVariant
{
    public class GetProductVariantsByProductDto
    {
        public ProductDto Product { get; set; }
        public IEnumerable<ProductVariantDto> ProductVariants { get; set; } = new List<ProductVariantDto>();
        public string UserId { get; set; }
    }
}
