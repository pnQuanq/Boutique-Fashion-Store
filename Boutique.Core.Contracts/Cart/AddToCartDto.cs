namespace Boutique.Core.Contracts.Cart
{
    public class AddToCartDto
    {
        public string UserId { get; set; }
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }
}
