﻿using AutoMapper;
using Boutique.Core.Contracts.Cart;
using Boutique.Core.Contracts.Category;
using Boutique.Core.Contracts.Discount;
using Boutique.Core.Contracts.Order;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Contracts.ProductVariant;
using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Services.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Category Mapping Profile
            CreateMap<Category, CategoryDto>();
            CreateMap<Category, CreateCategoryDto>()
                .ReverseMap();

            CreateMap<Category, UpdateCategoryDto>()
                .ReverseMap();
            //Product Mapping Profile
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            CreateMap<ProductImage, ProductImageDto>();

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());
            //ProductVariant Mapping Profile
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.Size.Name))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color.Name))
                .ForMember(dest => dest.Hex, opt => opt.MapFrom(src => src.Color.Hex));
            //Cart Mapping Profile
            CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemDto>();

            CreateMap<AddToCartDto, CartItem>()
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.Discount, opt => opt.Ignore());

            CreateMap<UpdateCartItemDto, CartItem>()
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());
            //Discount Mapping Profile
            CreateMap<CreateDiscountDto, Discount>();

            CreateMap<Discount, DiscountDto>().ReverseMap();
            //Order Mapping Profile
            CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            // Map OrderItem to OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.ProductVariant.Size.Name))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.ProductVariant.Color.Name));

            CreateMap<Order, OrderHistoryDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.DateCreated)) // Assuming CreatedAt is the order date
            .ForMember(dest => dest.AddressValue, opt => opt.MapFrom(src => src.AddressValue))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentStatus)) // Assuming PaymentMethod is same as PaymentStatus, adjust as needed
            .ForMember(dest => dest.PayAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.OrderStatus))
            .ForMember(dest => dest.RecipientName, opt => opt.MapFrom(src => src.RecipientName))
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
        }
    }
}
