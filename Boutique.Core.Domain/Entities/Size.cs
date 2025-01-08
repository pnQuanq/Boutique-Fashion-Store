using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Size : BaseEntity
    {
        public int SizeId { get; set; }
        public required string Name { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; }
    }
}
