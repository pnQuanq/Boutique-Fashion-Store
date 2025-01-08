using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Color : BaseEntity
    {
        public int ColorId { get; set; }
        public required string Name { get; set; }
        public string Hex {  get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; }
    }
}
