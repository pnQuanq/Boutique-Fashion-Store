using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Order : BaseEntity
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SubTotal {  get; set; }
        public decimal DeliveryFee { get; set; }
        public required string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public required string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string RecipientName { get; set; }
        public string AddressValue { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
