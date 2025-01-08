using Boutique.Core.Contracts.Cart;
using Boutique.Core.Contracts.Category;
using Boutique.Core.Contracts.Product;

namespace Boutique.Web.ViewModel
{
    public class ProductHomeViewModel
    {
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public CartDto cartDto { get; set; }
    }
}
