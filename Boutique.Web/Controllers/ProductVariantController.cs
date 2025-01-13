using Microsoft.AspNetCore.Mvc;
using Boutique.Core.Contracts.ProductVariant;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Web.ViewModel.ProductVariant;
using Microsoft.AspNetCore.Authorization;

namespace Boutique.Web.Controllers
{
    public class ProductVariantController : Controller
    {
        private readonly IProductVariantService _productVariantService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        public ProductVariantController(IProductVariantService productVariantService, IProductService productService, ICategoryService categoryService)
        {
            _productVariantService = productVariantService;
            _productService = productService;
            _categoryService = categoryService;
        }
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ProductVariantHome()
        {
            var productVariants = await _productVariantService.GetAllProductVariantsAsync();
            var products = await _productService.GetAllProductsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var viewModel = new ProductVariantViewModel
            {
                ProductVariants = productVariants,
                Products = products,
                Categories = categories
            };

            return View(viewModel);
        }
        [Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProductVariant(CreateProductVariantDto createDto)
        {
            try
            {
                var result = await _productVariantService.CreateAsync(createDto);

                TempData["SuccessMessage"] = "Product Variant created successfully!";
                return RedirectToAction("ProductVariantHome");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("ProductVariantHome");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductVariantById(int id)
        {
            var result = await _productVariantService.GetProductVariantByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductVariants()
        {
            var result = await _productVariantService.GetAllProductVariantsAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductVariant([FromForm] UpdateProductVariantDto updateDto)
        {
            try
            {
                var updatedProductVariant = await _productVariantService.UpdateProductVariantAsync(updateDto.ProductVariantId, updateDto);
                TempData["SuccessMessage"] = "Product variant updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to update this product variant: {ex.Message}";
            }

            return RedirectToAction("ProductVariantHome");
        }

        [HttpDelete("{productVariantId}")]
        public async Task<IActionResult> DeleteProductVariantById(int productVariantId)
        {
            try
            {
                var isDeleted = await _productVariantService.DeleteProductVariantByIdAsync(productVariantId);
                if (isDeleted)
                {
                    return NoContent();
                }

                return NotFound("Product variant not found");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

    }
}
