using AutoMapper;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Core.Services.Exceptions;
using Boutique.Core.Domain.Repositories;

namespace Boutique.Core.Services.Features
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IMapper _mapper;
        private readonly string _imageFolderPath = "wwwroot/images/products";
        private Dictionary<string, double> _termWeights = new();
        public ProductService(IProductRepository productRepository, IProductImageRepository productImageRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();

            return products.Select(product => new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Images = _mapper.Map<ICollection<ProductImageDto>>(product.Images),
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            });
        }
        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.GetProductWithCategoryAsync(productId);

            if (product == null)
            {
                throw new Exception($"Product with ID {productId} not found.");
            }

            return _mapper.Map<ProductDto>(product);
        }
        public async Task<ProductDto> AddProductAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);

            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }

            product.Images = product.Images ?? new List<ProductImage>();

            foreach (var image in createProductDto.Images)
            {
                if (image != null && image.Length > 0)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
                    var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    product.Images.Add(new ProductImage { ImagePath = $"/images/products/{uniqueFileName}" });
                }
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            return _mapper.Map<ProductDto>(product);
        }
        public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
        {
            var product = await _productRepository.GetProductWithCategoryAsync(productId);

            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            product.Name = updateProductDto.Name;
            product.Description = updateProductDto.Description;
            product.Price = updateProductDto.Price;
            product.CategoryId = updateProductDto.CategoryId;

            if (updateProductDto.ImagesToRemove != null && updateProductDto.ImagesToRemove.Any())
            {
                var imagesToRemove = product.Images.Where(img => updateProductDto.ImagesToRemove.Contains(img.ProductImageId)).ToList();
                foreach (var image in imagesToRemove)
                {
                    await _productImageRepository.DeleteAsync(image.ProductImageId);
                }
            }

            if (updateProductDto.ImagesToAdd != null && updateProductDto.ImagesToAdd.Any())
            {
                foreach (var image in updateProductDto.ImagesToAdd)
                {
                    if (image != null && image.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
                        var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        product.Images.Add(new ProductImage { ImagePath = $"/images/products/{uniqueFileName}" });
                    }
                }
            }

            await _productRepository.SaveAsync();

            return _mapper.Map<ProductDto>(product);
        }
        public async Task<string> DeleteProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                throw new Exception($"Product with ID {productId} not found.");
            }

            foreach (var image in product.Images)
            {
                var filePath = Path.Combine(_imageFolderPath, image.ImagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            await _productRepository.DeleteAsync(productId);
            await _productRepository.SaveAsync();

            return $"Product with ID {productId} has been successfully deleted.";
        }
        public async Task<IEnumerable<Product>> GetAllProductsForRecomendSystemAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            return products;
        }
        public async Task<IEnumerable<ProductDto>> GetRecentProductsAsync(int count)
        {
            var products =  await _productRepository.GetGetRecentProductsAsync(count);
            return products.Select(product => new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Images = _mapper.Map<ICollection<ProductImageDto>>(product.Images),
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            });
        }
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAndGenderAsync(int categoryId, int gender)
        {
            var products = await _productRepository.GetProductsByCategoryAndGenderAsync(categoryId, gender);

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        public async Task<IEnumerable<ProductDto>> GetProductsByGenderAsync(int gender)
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            products = products.Where(p => p.Category.Gender == gender)
                            .ToList();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        //public async Task<List<ProductDto>> SearchProductsByNameAsync(string searchString)
        //{
        //    if (string.IsNullOrWhiteSpace(searchString))
        //    {
        //        throw new ArgumentException("Search string cannot be empty.", nameof(searchString));
        //    }

        //    var products = await _productRepository.SearchProductsByNameAsync(searchString);

        //    if (!products.Any())
        //    {
        //        throw new Exception($"No products found containing '{searchString}'.");
        //    }

        //    return _mapper.Map<List<ProductDto>>(products);
        //}
        public async Task InitializeAutoTFSWeightsAsync()
        {
            //Lấy toàn bộ sản phẩm
            var products = await _productRepository.GetAllWithCategoryAsync();

            var keywordCounts = new Dictionary<string, int>();
            int total = 0;

            foreach (var p in products)
            {
                var text = $"{p.Name} {p.Category?.Name}".ToLower();
                var words = text.Split(new[] { ' ', '-', '_', '.', ',', ':' }, StringSplitOptions.RemoveEmptyEntries).Distinct();

                foreach (var word in words)
                {
                    if (!keywordCounts.ContainsKey(word))
                        keywordCounts[word] = 0;

                    keywordCounts[word]++;
                    total++;
                }
            }

            _termWeights = keywordCounts.ToDictionary(
                x => x.Key,
                x => (double)x.Value / total
            );
        }

        public async Task<List<ProductDto>> SearchProductsByNameAsync(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                throw new ArgumentException("Search string cannot be empty.", nameof(searchString));

            var allProducts = await _productRepository.GetAllWithCategoryAsync(); // đảm bảo có Category

            if (!allProducts.Any())
                throw new Exception("No products available.");

            var keywords = searchString.ToLower()
                .Split(new[] { ' ', ',', '.', '-', '_', ':' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();

            var ranked = allProducts
                .Select(p => new
                {
                    Product = p,
                    Score = CalculateNBEMScore(p, keywords)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Product)
                .ToList();

            if (!ranked.Any())
                throw new Exception($"No products found related to '{searchString}'.");

            return _mapper.Map<List<ProductDto>>(ranked);
        }

        private double CalculateNBEMScore(Product product, List<string> keywords)
        {
            double score = 0;

            foreach (var kw in keywords)
            {
                double weight = _termWeights.TryGetValue(kw, out var w) ? w : 0.01;

                // Bernoulli: product name
                if (product.Name?.ToLower().Contains(kw) == true)
                    score += 0.3 * weight;

                // Categorical: category name
                if (product.Category?.Name?.ToLower().Contains(kw) == true)
                    score += 0.5 * weight;

                // Gaussian: giá (nếu từ khóa là số)
                if (decimal.TryParse(kw, out decimal targetPrice))
                {
                    var diff = Math.Abs(product.Price - targetPrice);
                    score += 0.2 * weight * (1.0 / (1.0 + (double)diff));
                }
            }

            return score;
        }
    }
}
