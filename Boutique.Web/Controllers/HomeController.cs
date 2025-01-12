using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boutique.Web.Controllers
{
	public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductVariantService _productVariantService;

        public HomeController(IProductService productService, ICategoryService categoryService, IProductVariantService productVariantService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productVariantService = productVariantService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            var viewModel = new ProductHomeViewModel
            {
                Products = products,
                Categories = categories
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
