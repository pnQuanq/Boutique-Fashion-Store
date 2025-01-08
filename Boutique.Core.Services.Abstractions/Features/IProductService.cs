﻿using Boutique.Core.Contracts.Product;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IProductService
    {
        Task<ProductDto> AddProductAsync(CreateProductDto productDto);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateProductDto);
        Task<string> DeleteProductAsync(int productId);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int productId);
    }
}
