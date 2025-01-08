using Boutique.Core.Contracts.Category;

namespace Boutique.Web.ViewModel
{
    public class CategoryHomeViewModel
    {
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }
}
