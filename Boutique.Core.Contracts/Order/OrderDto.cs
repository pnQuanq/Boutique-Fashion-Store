using Boutique.Core.Contracts.Order;

namespace Boutique.Core.Contracts.Order
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string RecipientName { get; set; }
        public decimal TotalAmount { get; set; } = 0;
        public decimal DeliveryFee { get; set; } = 0;
        public decimal SubTotal { get; set; } = 0;
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string UserId { get; set; }
        public string AddressValue { get; set; }
        public string PaymentMethod { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public DateTime DateCreated { get; set; }

    }
}
