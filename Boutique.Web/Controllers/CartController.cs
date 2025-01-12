using Microsoft.AspNetCore.Mvc;
using Boutique.Core.Contracts.Cart;
using Boutique.Core.Services.Abstractions.Features;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Boutique.Web.ViewModel.Cart;

namespace Boutique.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        [HttpPost("Add-To-Cart")]
        public async Task<IActionResult> AddToCartAsync([FromBody] AddToCartDto addToCartDto)
        {
			if (!User.Identity.IsAuthenticated)
			{
				return Unauthorized("User not authenticated");
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            addToCartDto.UserId = userId;

            try
            {
                var updatedCart = await _cartService.AddToCartAsync(addToCartDto);

                if (updatedCart == null)
                {
                    return BadRequest("Failed to add item to cart");
                }

				return Ok(new
				{
					success = true,
					cart = updatedCart
				});
			}
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("update-cart")]
        public async Task<IActionResult> UpdateCartItemAsync([FromForm] UpdateCartItemDto updateDto)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var result = await _cartService.UpdateCartItemAsync(updateDto.CartItemId, updateDto);
                if (result)
                {
                    var updatedCart = await _cartService.GetCartByUserIdAsync(uid);
                    return RedirectToAction("CartPage", new { userId = uid });
                }

                return NotFound("Cart item not found");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItemAsync(int cartItemId)
        {
            try
            {
                var result = await _cartService.RemoveCartItemAsync(cartItemId);
                if (result)
                {
                    return Ok("Remove cart item successfully");
                }

                return NotFound("Cart item not found");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("cart")]
        public async Task<IActionResult> CartPage()
        {
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			try
            {
                var cartDto = await _cartService.GetCartByUserIdAsync(uid);
                if (cartDto == null)
                {
                    return RedirectToAction("EmptyCart");
                }
                var model = new CartViewModel();
                model.Cart = cartDto;
                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("empty-cart")]
        public IActionResult EmptyCart()
        {
            return View();
        }
    }
}
