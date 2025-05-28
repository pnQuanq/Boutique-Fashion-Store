using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Features.ProductSearchService.PriceProcessing;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;

namespace Boutique.Core.Services.Features.ProductSearchService.SearchStrategies
{
    public class PriceOnlySearchStrategy
    {
        private readonly PriceAnalyzer _priceAnalyzer;

        public PriceOnlySearchStrategy(PriceAnalyzer priceAnalyzer)
        {
            _priceAnalyzer = priceAnalyzer;
        }

        public List<Product> Search(IEnumerable<Product> products, decimal priceQuery)
        {
            return products
                .Select(p => new ProductScore
                {
                    Product = p,
                    PriceScore = _priceAnalyzer.CalculatePriceProximityScore(p.Price, priceQuery)
                })
                .OrderByDescending(x => x.PriceScore)
                .Take(SearchConstants.MAX_RESULTS)
                .Select(x => x.Product)
                .ToList();
        }
    }
}
