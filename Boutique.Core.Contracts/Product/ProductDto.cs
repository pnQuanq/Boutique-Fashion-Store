﻿namespace Boutique.Core.Contracts.Product
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public ICollection<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
    public class ProductImageDto
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; }
    }
}
