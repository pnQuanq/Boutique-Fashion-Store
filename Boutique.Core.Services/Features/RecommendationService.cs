using AutoMapper;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Domain.RecommendProfile;
using Boutique.Core.Services.Abstractions.Features;

namespace Boutique.Core.Services.Features
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public RecommendationService(IProductService productService,
                                     IOrderService orderService,
                                     ICategoryService categoryService,
                                     IMapper mapper)
        {
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
            _mapper = mapper;
            Task.Run(async () => await InitializeCategoryMapAsync()).Wait();
        }

        public async Task<ProductRecommendationDto> GetRecommendationsForUserAsync(string userId)
        {
            var userOrders = await _orderService.GetOrderHistoryByUserIdAsync(userId);

            if (!userOrders.Any())
            {
                var products = await _productService.GetRecentProductsAsync(10);

                var recentProducts = products.Select(productDto => new RecommendedProductDto
                {
                    Product = productDto,
                    RelevanceScore = 0
                }).ToList();

                return new ProductRecommendationDto
                {
                    Products = recentProducts
                };
            }

            var userProfile = BuildUserProfile(userOrders);

            var allProducts = await _productService.GetAllProductsForRecomendSystemAsync();
            var recommendedProducts = new List<RecommendedProductDto>();

            foreach (var product in allProducts)
            {
                var productProfile = BuildProductProfile(product);
                var similarity = CalculateSimilarity(userProfile, productProfile);

                if (similarity > 0.5)
                {
                    recommendedProducts.Add(new RecommendedProductDto
                    {
                        Product = new ProductDto
                        {
                            ProductId = product.ProductId,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            CategoryId = product.Category.CategoryId,
                            CategoryName = product.Category.Name,
                            Images = _mapper.Map<ICollection<ProductImageDto>>(product.Images),

                        },
                        RelevanceScore = similarity
                    });
                }
            }

            return new ProductRecommendationDto
            {
                Products = recommendedProducts.OrderByDescending(p => p.RelevanceScore).ToList()
            };
        }

        private UserProfile BuildUserProfile(IEnumerable<Order> orders)
        {
            var userProfile = new UserProfile();
            var categoryCount = new Dictionary<string, int>();
            decimal totalPrice = 0;
            int totalItems = 0;

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product?.Category?.Name != null)
                    {
                        if (categoryCount.ContainsKey(item.Product.Category.Name))
                        {
                            categoryCount[item.Product.Category.Name]++;
                        }
                        else
                        {
                            categoryCount[item.Product.Category.Name] = 1;
                        }
                    }

                    totalPrice += item.Product.Price;
                    totalItems++;
                }
            }

            // Handle sparse data
            userProfile.MostFrequentCategory = categoryCount.OrderByDescending(c => c.Value)
                                                             .FirstOrDefault().Key ?? "DefaultCategory";

            userProfile.AveragePrice = totalItems > 0 ? (double)totalPrice / totalItems : 0;

            var preferredGender = orders.SelectMany(o => o.OrderItems)
                                         .Where(i => i.Product?.Category != null)
                                         .GroupBy(i => i.Product.Category.Gender)
                                         .OrderByDescending(g => g.Count())
                                         .FirstOrDefault()?.Key ?? 0; // Default gender value
            userProfile.PreferredGender = preferredGender;

            return userProfile;
        }


        private ProductProfile BuildProductProfile(Product product)
        {
            var productProfile = new ProductProfile
            {
                Category = product.Category.Name,
                Gender = product.Category.Gender,
                Price = (double)product.Price
            };

            return productProfile;
        }

        private double CalculateSimilarity(UserProfile userProfile, ProductProfile productProfile)
        {
            var userCategoryVector = ConvertCategoryToVector(userProfile.MostFrequentCategory);
            var productCategoryVector = ConvertCategoryToVector(productProfile.Category);

            double categorySimilarity = userCategoryVector.Zip(productCategoryVector, (a, b) => a * b).Sum();

            // Assign higher weight to price and gender similarity if category similarity is sparse
            double priceDifference = Math.Abs(userProfile.AveragePrice - productProfile.Price);
            double priceSimilarity = 1 - (priceDifference / (userProfile.AveragePrice + productProfile.Price + 1)); // Normalize

            double genderSimilarity = userProfile.PreferredGender == productProfile.Gender ? 1 : 0;

            // Weights can be adjusted based on experiments
            return (0.5 * categorySimilarity) + (0.3 * priceSimilarity) + (0.2 * genderSimilarity);
        }

        private double CosineSimilarity(List<double> vectorA, List<double> vectorB)
        {
            var dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
            var magnitudeA = Math.Sqrt(vectorA.Sum(x => x * x));
            var magnitudeB = Math.Sqrt(vectorB.Sum(x => x * x));
            return dotProduct / (magnitudeA * magnitudeB);
        }

        private Dictionary<string, int> _categoryMap;

        public async Task InitializeCategoryMapAsync()
        {
            if (_categoryMap != null) return;
            var categories = await _categoryService.GetAllCategoriesForRemcommendAsync();
            _categoryMap = categories.Select((category, index) => new { category.Name, Index = index })
                                      .ToDictionary(c => c.Name, c => c.Index);
        }

        private List<double> ConvertCategoryToVector(string category)
        {
            var vector = new List<double>(new double[_categoryMap.Count]);
            if (_categoryMap.TryGetValue(category, out var index))
            {
                vector[index] = 1;
            }
            return vector;
        }

    }
}
