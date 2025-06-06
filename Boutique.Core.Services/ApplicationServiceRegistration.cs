﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Core.Services.Features;
using Boutique.Core.Services.Mapper;
using Boutique.Core.Services.Features.ProductSearchService;

namespace Boutique.Core.Services
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductVariantService, ProductVariantService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductSearchService, ProductSearchService>();
            return services;
        }
    }
}
