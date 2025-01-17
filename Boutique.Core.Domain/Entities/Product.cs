﻿using Boutique.Core.Domain.Common;

namespace Boutique.Core.Domain.Entities
{
    public class Product : BaseEntity
    {
        public int ProductId { get; set; }
        public required string Name { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; }
        public ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();

    }
}
