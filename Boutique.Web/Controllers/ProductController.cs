using Microsoft.AspNetCore.Mvc;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Core.Services.Exceptions;
using Boutique.Web.ViewModel;
using Boutique.Web.ViewModel.Product;

namespace Boutique.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> ProductHome()
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
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                var newProduct = await _productService.AddProductAsync(createProductDto);

                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction("ProductHome");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("ProductHome");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryName = product.CategoryName,
                Images = product.Images,
                DateCreated = product.DateCreated,
                DateModified = product.DateModified
            };

            return View(viewModel);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromForm] UpdateProductDto updateProductDto)
        {
            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(updateProductDto.ProductId, updateProductDto);

                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("ProductHome");
            }
            catch (NotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ProductHome");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("ProductHome");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                await _productService.DeleteProductAsync(productId);
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to delete this product: {ex.Message}";
            }

            return RedirectToAction("ProductHome");
        }
        public IActionResult ProductDetail()
        {
            return View();
        }

    }
}
