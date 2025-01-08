using AutoMapper;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Contracts.ProductVariant;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Core.Domain.Repositories;

namespace Boutique.Core.Services.Features
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISizeRepository _sizeRepository;
        private readonly IColorRepository _colorRepository;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        public ProductVariantService(
            IProductVariantRepository productVariantRepository,
            IProductRepository productRepository,
            IColorRepository colorRepository,
            ISizeRepository sizeRepository,
            IMapper mapper,
            IProductService productService)
        {
            _productVariantRepository = productVariantRepository;
            _productRepository = productRepository;
            _colorRepository = colorRepository;
            _sizeRepository = sizeRepository;
            _mapper = mapper;
            _productService = productService;
        }
        public async Task<ProductVariantDto> CreateAsync(CreateProductVariantDto createDto)
        {
            var product = await _productRepository.GetByIdAsync(createDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            var size = new Size
            {
                Name = createDto.SizeName
            };
            await _sizeRepository.AddAsync(size);

            var color = new Color
            {
                Name = createDto.ColorName,
                Hex = createDto.Hex
            };
            await _colorRepository.AddAsync(color);

            var productVariant = new ProductVariant
            {
                ProductId = createDto.ProductId,
                SizeId = size.SizeId,
                ColorId = color.ColorId,
                Quantity = createDto.Quantity
            };

            await _productVariantRepository.AddAsync(productVariant);

            product.Quantity += productVariant.Quantity;
            await _productRepository.UpdateAsync(product);

            return _mapper.Map<ProductVariantDto>(productVariant);
        }

        public async Task<ProductVariantDto> GetProductVariantByIdAsync(int id)
        {
            var productVariant = await _productVariantRepository.GetProductVariantByIdAsync(id);

            if (productVariant == null)
                throw new KeyNotFoundException("Product variant not found");

            var product = await _productService.GetProductByIdAsync(productVariant.ProductId);

            var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);

            productVariantDto.Product = _mapper.Map<ProductDto>(product);

            return productVariantDto;
        }
        public async Task<IEnumerable<ProductVariantDto>> GetAllProductVariantsAsync()
        {
            var productVariantDtos = new List<ProductVariantDto>();

            var productVariants = await _productVariantRepository.GetAllProductVariantsAsync();
            foreach (var variant in productVariants)
            {
                var product = await _productService.GetProductByIdAsync(variant.ProductId);
                var productVariantDto = _mapper.Map<ProductVariantDto>(variant);

                productVariantDto.Product = _mapper.Map<ProductDto>(product);
                productVariantDtos.Add(productVariantDto);
            }

            return productVariantDtos;
        }
        public async Task<ProductVariantDto> UpdateProductVariantAsync(int id, UpdateProductVariantDto updateDto)
        {
            var productVariant = await _productVariantRepository.GetProductVariantByIdAsync(id);

            if (productVariant == null)
            {
                throw new KeyNotFoundException("Product variant not found");
            }

            var size = await _sizeRepository.GetByIdAsync(productVariant.SizeId);
            var color = await _colorRepository.GetByIdAsync(productVariant.ColorId);

            if (size == null)
            {
                throw new KeyNotFoundException("Size not found");
            }

            if (color == null)
            {
                throw new KeyNotFoundException("Color not found");
            }

            var product = await _productRepository.GetByIdAsync(productVariant.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            int oldQuantity = productVariant.Quantity;
            int newQuantity = updateDto.Quantity;
            int quantityDifference = newQuantity - oldQuantity;

            size.Name = updateDto.SizeName;

            color.Name = updateDto.ColorName;
            color.Hex = updateDto.Hex;

            productVariant.Quantity = newQuantity;

            await _sizeRepository.UpdateAsync(size);
            await _colorRepository.UpdateAsync(color);
            await _productVariantRepository.UpdateAsync(productVariant);

            product.Quantity += quantityDifference;
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveAsync();

            var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
            productVariantDto.Product = _mapper.Map<ProductDto>(product);

            return productVariantDto;
        }

        public async Task<bool> DeleteProductVariantByIdAsync(int id)
        {
            var productVariant = await _productVariantRepository.GetProductVariantByIdAsync(id);

            if (productVariant == null)
            {
                throw new KeyNotFoundException("Product variant not found");
            }

            var size = await _sizeRepository.GetByIdAsync(productVariant.SizeId);
            var color = await _colorRepository.GetByIdAsync(productVariant.ColorId);

            await _productVariantRepository.DeleteAsync(productVariant.ProductId);

            await _sizeRepository.DeleteAsync(size.SizeId);
            await _colorRepository.DeleteAsync(color.ColorId);

            return true;
        }
        public async Task<GetProductVariantsByProductDto> GetProductAndProductVariantsAsync(int id)
        {
            var product = await _productRepository.GetProductWithCategoryAsync(id);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }
            var productVariants = await _productVariantRepository.GetProductVariantsByProductIdAsync(id);

            if (productVariants == null || !productVariants.Any())
            {
                throw new KeyNotFoundException("No variants found for this product");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            var productVariantDtos = _mapper.Map<IEnumerable<ProductVariantDto>>(productVariants);

            return new GetProductVariantsByProductDto
            {
                Product = productDto,
                ProductVariants = productVariantDtos
            };
        }
    }
}
