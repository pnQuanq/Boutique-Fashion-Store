using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public int CartId { get; set; }
        public required string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
