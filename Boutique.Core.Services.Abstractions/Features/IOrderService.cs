using Boutique.Core.Contracts.Order;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(string userId);
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status);
        Task DeleteOrderAsync(int orderId);
    }
}
