﻿using System;
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
        public int AddressId { get; set; }
        public Address Address { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
