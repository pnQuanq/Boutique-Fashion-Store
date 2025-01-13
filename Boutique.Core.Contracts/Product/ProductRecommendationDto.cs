namespace Boutique.Core.Contracts.Product
{
    public class ProductRecommendationDto
    {
        public List<RecommendedProductDto> Products { get; set; }
    }

    public class RecommendedProductDto
    {
        public ProductDto Product { get; set; }
        public double RelevanceScore { get; set; }
    }

}
