using Boutique.Core.Contracts.Order;

namespace Boutique.Web.ViewModel.Order
{
    public class OrderHistoryViewModel
    {
        public IEnumerable<OrderHistoryDto> OrderHistory { get; set; } = new List<OrderHistoryDto>();
        public OrderDto Order { get; set; }
    }
}
