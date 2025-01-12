using AutoMapper;
using Boutique.Core.Contracts.Order;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Domain.Repositories;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Infrastructure.Persistence.DataContext;

namespace Boutique.Core.Services.Features
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public OrderService(IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IMapper mapper,
            ApplicationDbContext context,
            IProductVariantRepository productVariantRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _mapper = mapper;
            _context = context;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
        }
        public async Task<OrderDto> CreateOrderAsync(string uid, CreateOrderDto dto)
        {
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(uid);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Cart is empty or not found.");
            }

            decimal totalAmount = 0;
            foreach (var cartItem in cart.CartItems)
            {
                totalAmount += cartItem.UnitPrice * cartItem.Quantity;

                var productVariant = await _productVariantRepository.GetByIdAsync(cartItem.ProductVariantId);

                var product = await _productRepository.GetByIdAsync(productVariant.ProductId);

                productVariant.Quantity -= cartItem.Quantity;
                product.Quantity -= cartItem.Quantity;

                await _productRepository.UpdateAsync(product);
                await _productVariantRepository.UpdateAsync(productVariant);
            }

            var status = "";

            if (dto.PaymentMethod == "Ship COD")
            {
                status = "Pending";
            }
            else
            {
                status = "Paid";
            }

            var order = new Order
            {
                TotalAmount = totalAmount,
                PaymentStatus = status,
                OrderStatus = "Order Confirmed",
                UserId = uid,
                AddressValue = dto.AddressName,
                RecipientName = dto.RecipientName,
                PhoneNumber = dto.PhoneNumber,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductVariant.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.ProductVariant.Product.Price,
                    ProductVariantId = ci.ProductVariantId
                }).ToList()
            };

            await _orderRepository.AddAsync(order);

            cart.CartItems.Clear();
            await _cartRepository.UpdateAsync(cart);

            var orderDto = _mapper.Map<OrderDto>(order);

            return orderDto;
        }
        public async Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersWithTransactionsByUserIdAsync(userId);

            var orderHistoryDtos = _mapper.Map<IEnumerable<OrderHistoryDto>>(orders);

            return orderHistoryDtos;
        }
        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithTransactionAndItemsAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            var orderDetailDto = _mapper.Map<OrderDto>(order);

            return orderDetailDto;
        }
        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllWithDetailsAsync();
            return _mapper.Map<List<OrderDto>>(orders);
        }
        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new Exception("Order not found.");

            //order.Status = status;
            await _orderRepository.UpdateAsync(order);

            return await GetOrderByIdAsync(orderId);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found.");

            _context.OrderItems.RemoveRange(order.OrderItems);


            await _orderRepository.DeleteAsync(order.OrderId);
            await _orderRepository.SaveAsync();
        }
    }
}