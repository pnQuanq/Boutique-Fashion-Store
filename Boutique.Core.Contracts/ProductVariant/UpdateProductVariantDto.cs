namespace Boutique.Core.Contracts.ProductVariant
{
    public class UpdateProductVariantDto
    {
        public int ProductVariantId { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
        public string Hex {  get; set; }
        public int Quantity { get; set; }
    }
}
