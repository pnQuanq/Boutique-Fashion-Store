using Boutique.Core.Contracts.Category;
using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface ICategoryService
    {
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesForRemcommendAsync();
        Task<CategoryDto> AddCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<string> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task DeleteCategoryAsync(int id);
    }
}
