﻿namespace Boutique.Core.Contracts.ProductVariant
{
    public class CreateProductVariantDto
    {
        public int ProductId { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
        public string Hex {  get; set; }
        public int Quantity { get; set; }
    }
}
