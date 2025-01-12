namespace Boutique.Core.Contracts.Order
{
    public class CreateOrderDto
    {
        public string AddressName { get; set; }
        public string PhoneNumber { get; set; }
        public string RecipientName { get; set; }
        public string PaymentMethod { get; set; }
    }
}
