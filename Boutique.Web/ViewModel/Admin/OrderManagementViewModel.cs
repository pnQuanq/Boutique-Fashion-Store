using Boutique.Core.Contracts.Order;

namespace Boutique.Web.ViewModel.Admin
{
    public class OrderManagementViewModel
    {
        public List<OrderDto> Orders { get; set; } = new List<OrderDto> { new OrderDto() };
    }
}
