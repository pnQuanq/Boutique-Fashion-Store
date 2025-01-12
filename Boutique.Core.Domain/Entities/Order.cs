using System;
using System.Collections.Generic;
using System.Linq;
using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Order : BaseEntity
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public required string Status { get; set; }
        public required string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string RecipientName { get; set; }
        public string AddressValue { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Transaction Transaction { get; set; }
    }
}
