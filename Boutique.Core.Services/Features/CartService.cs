﻿using AutoMapper;
using Boutique.Core.Contracts.Cart;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Domain.Repositories;
using Boutique.Core.Services.Abstractions.Features;

namespace Boutique.Core.Services.Features
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IMapper _mapper;

        public CartService(
            ICartRepository cartRepository,
            IProductVariantRepository productVariantRepository,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productVariantRepository = productVariantRepository;
            _mapper = mapper;
        }

        public async Task<CartDto> GetCartByUserIdAsync(string userId)
        {
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

            if (cart == null) return null;

            foreach (var cartItem in cart.CartItems)
            {
                var productVariant = await _productVariantRepository.GetProductVariantByIdAsync(cartItem.ProductVariantId);
                if (productVariant != null)
                {
                    cartItem.UnitPrice = productVariant.Product.Price;
                }
            }

            await _cartRepository.SaveAsync();

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto addToCartDto)
        {
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(addToCartDto.UserId);

            if (cart == null)
            {
                cart = new Cart { UserId = addToCartDto.UserId, CartItems = new List<CartItem>() };
                await _cartRepository.AddAsync(cart);
            }

            var productVariant = await _productVariantRepository.GetProductVariantByIdAsync(addToCartDto.ProductVariantId);
            if (productVariant == null) throw new KeyNotFoundException("Product variant not found");

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantId == addToCartDto.ProductVariantId);

            if (existingCartItem != null)
            {

                existingCartItem.Quantity += addToCartDto.Quantity;
                existingCartItem.UnitPrice = productVariant.Product.Price; 
                existingCartItem.Discount = addToCartDto.Discount;
            }
            else
            {
                var cartItem = _mapper.Map<CartItem>(addToCartDto);
                cartItem.ProductVariant = productVariant;
                cartItem.UnitPrice = productVariant.Product.Price;
                cartItem.Discount = addToCartDto.Discount;
                
                cart.CartItems.Add(cartItem);
            }

            await _cartRepository.SaveAsync();

            return _mapper.Map<CartDto>(cart); 
        }
        public async Task<bool> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto updateDto)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Cart item not found");
            }

            cartItem.Quantity = updateDto.Quantity;

            await _cartRepository.SaveAsync();
            return true;
        }
        public async Task<bool> RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Cart item not found");
            }

            await _cartRepository.DeleteCartItemAsync(cartItemId);

            await _cartRepository.SaveAsync();
            return true;
        }
    }
}
