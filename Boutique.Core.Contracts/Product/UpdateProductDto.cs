using Microsoft.AspNetCore.Http;
namespace Boutique.Core.Contracts.Product
{
    public class UpdateProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public List<IFormFile> ImagesToAdd { get; set; } = new List<IFormFile>();
        public ICollection<int> ImagesToRemove { get; set; } = new List<int>();
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
