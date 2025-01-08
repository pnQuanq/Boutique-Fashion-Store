using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Category : BaseEntity
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public int Gender { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
