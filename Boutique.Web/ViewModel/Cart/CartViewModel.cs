using Boutique.Core.Contracts.Cart;

namespace Boutique.Web.ViewModel.Cart
{
	public class CartViewModel
	{
		public CartDto Cart { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal SubTotal {  get; set; }
    }
}
