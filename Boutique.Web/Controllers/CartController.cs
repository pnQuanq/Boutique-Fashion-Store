using Microsoft.AspNetCore.Mvc;
using Boutique.Core.Contracts.Cart;
using Boutique.Core.Services.Abstractions.Features;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Boutique.Web.ViewModel;

namespace Boutique.API.Controllers
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
        [HttpPost]
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

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItemAsync(int cartItemId, [FromBody] UpdateCartItemDto updateDto)
        {
            try
            {
                var result = await _cartService.UpdateCartItemAsync(cartItemId, updateDto);
                if (result)
                {
                    var updatedCart = await _cartService.GetCartByUserIdAsync(updateDto.UserId);
                    return Ok(updatedCart);
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
        [HttpGet("cart/{userId}")]
        public async Task<IActionResult> GetCartByUserIdAsync(string userId)
        {
			var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			try
            {
                var cartDto = await _cartService.GetCartByUserIdAsync(uid);
                if (cartDto == null)
                {
                    return NotFound("Cart not found");
                }
                var model = new ProductHomeViewModel();
                model.cartDto = cartDto;
                return View("CartPage");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
