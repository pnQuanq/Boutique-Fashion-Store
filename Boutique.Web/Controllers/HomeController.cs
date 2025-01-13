using Boutique.Core.Contracts.Category;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Web.ViewModel;
using Boutique.Web.ViewModel.Home;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Boutique.Web.Controllers
{
	public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductVariantService _productVariantService;
        private readonly IRecommendationService _recommendationService;

        public HomeController(IProductService productService,
                              ICategoryService categoryService,
                              IProductVariantService productVariantService,
                              IRecommendationService recommendationService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productVariantService = productVariantService;
            _recommendationService = recommendationService;
        }
        public async Task<IActionResult> Index()
        {
            string uid = null;
            if (User.Identity.IsAuthenticated)
            {               
                uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            var recentProducts = await _productService.GetRecentProductsAsync(10);
            var recommendProducts = await _recommendationService.GetRecommendationsForUserAsync(uid);

            var model = new IndexViewModel
            {
                RecentProducts = recentProducts,
                RecommendProducts = recommendProducts
            };

            return View(model);
        }
        public async Task<IActionResult> AllProducts(int? categoryId, int gender = 2, string searchString = null)
        {
            IEnumerable<ProductDto> products = Enumerable.Empty<ProductDto>();
            var selectedCategory = new CategoryDto();
            var selectedGender = 2;

            var categories = await _categoryService.GetAllCategoriesAsync();
            var model = new ProductHomeViewModel();

            if (searchString != null)
            {
                products = await _productService.SearchProductsByNameAsync(searchString);
                selectedCategory.Name = "All Categories";
                model.Products = products;
                model.Categories = categories;
                model.SelectedCategory = selectedCategory;
                return View(model);
            }

            if ((categoryId == null || categoryId == 0) && gender != 2)
            {
                products = await _productService.GetProductsByGenderAsync(gender);
                selectedCategory.Name = "All Categories";
                selectedGender = gender;
            }
            else if (categoryId == null || categoryId == 0)
            {
                products = await _productService.GetAllProductsAsync();
                selectedCategory.Name = "All Categories";
            }
            else if (categoryId != null && categoryId != 0 && gender == 2)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                selectedCategory = await _categoryService.GetCategoryByIdAsync(categoryId.Value);
            }
            else
            {
                products = await _productService.GetProductsByCategoryAndGenderAsync(categoryId.Value, gender);
                selectedCategory = await _categoryService.GetCategoryByIdAsync(categoryId.Value);
                selectedGender = gender;
            }


            var viewModel = new ProductHomeViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategory = selectedCategory,
                SelectedGender = selectedGender,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
			try
			{
				var product = await _productVariantService.GetProductAndProductVariantsAsync(id);
				if (product == null)
				{
					
					return NotFound("Product not found");
				}
                var model = new ProductHomeViewModel();
                model.ProductDetail = product;
				return View(model);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}
    }
}
