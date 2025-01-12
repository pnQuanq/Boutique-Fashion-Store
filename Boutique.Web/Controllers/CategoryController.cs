using Microsoft.AspNetCore.Mvc;
using Boutique.Core.Contracts.Category;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Web.ViewModel;

namespace Boutique.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryHome()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var viewModel = new CategoryHomeViewModel
            {
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CreateCategoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.AddCategoryAsync(request);

            if (category == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating category");
            }

            return RedirectToAction("CategoryHome");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromForm] UpdateCategoryDto request)
        {
            if (request == null || !ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data. Please check your inputs.";
                return RedirectToAction("CategoryHome");
            }
            try
            {
                string updateResult = await _categoryService.UpdateCategoryAsync(request.CategoryId, request);

                TempData["message"] = updateResult;

                return RedirectToAction("CategoryHome");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Internal server error: {ex.Message}";
                return RedirectToAction("CategoryHome");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["message"] = "Category deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete category: {ex.Message}";
            }

            return RedirectToAction("CategoryHome");
        }

    }
}
