using Boutique.Core.Contracts.Order;
using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<List<OrderHistoryDto>> GetOrderHistoryAsync(string userId);
        Task<IEnumerable<Order>> GetOrderHistoryByUserIdAsync(string userId);
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
        Task DeleteOrderAsync(int orderId);
    }
}
