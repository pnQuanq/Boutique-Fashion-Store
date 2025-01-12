using Boutique.Core.Contracts.Cart;
using Boutique.Core.Contracts.Order;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Web.ViewModel.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Boutique.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            try
            {
                // Fetch the user's cart using the service
                var cart = await _cartService.GetCartByUserIdAsync(uid);

                if (cart == null || !cart.CartItems.Any())
                {
                    return View(new CartViewModel
                    {
                        Cart = new CartDto(),
                        TotalCost = 0,
                        DeliveryFee = 0
                    });
                }

                var subTotal = cart.CartItems.Sum(item => item.UnitPrice * item.Quantity);

                decimal deliveryFee = subTotal > 100000 ? 15000 : 30000;

                var totalCost = subTotal + deliveryFee;

                var model = new CartViewModel
                {
                    Cart = cart,
                    DeliveryFee = deliveryFee,
                    TotalCost = totalCost,
                    SubTotal = subTotal
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto createOrderDto)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {

                if (string.IsNullOrEmpty(uid))
                {
                    return Unauthorized("User ID not found in token.");
                }

                var orderDto = await _orderService.CreateOrderAsync(uid, createOrderDto);

                return View("TrackOrder");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, innerException = ex.InnerException?.Message });
            }
        }
        [HttpGet]
        public IActionResult TrackOrder()
        {
            return View();
        }

    }
}
