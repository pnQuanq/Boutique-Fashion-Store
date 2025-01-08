using Boutique.Core.Contracts.Product;

namespace Boutique.Core.Contracts.ProductVariant
{
    public class ProductVariantDto
    {
        public int ProductVariantId { get; set; }
        public ProductDto Product { get; set; }
        public string SizeName { get; set; }
        public string Hex { get; set; }
        public string ColorName { get; set; }
        public int Quantity { get; set; }
    }
}
