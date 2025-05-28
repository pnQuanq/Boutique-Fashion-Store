using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Features.ProductSearchService.Models;
using Boutique.Core.Services.Features.ProductSearchService.NBEMEngine;
using Boutique.Core.Services.Features.ProductSearchService.PriceProcessing;
using Boutique.Core.Services.Features.ProductSearchService.TextProcessing;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;

namespace Boutique.Core.Services.Features.ProductSearchService.SearchStrategies
{
    public class CombinedSearchStrategy
    {
        private readonly NBEMScorer _nbemScorer;
        private readonly PriceAnalyzer _priceAnalyzer;
        private readonly TextAnalyzer _textAnalyzer;

        public CombinedSearchStrategy(NBEMScorer nbemScorer, PriceAnalyzer priceAnalyzer, TextAnalyzer textAnalyzer)
        {
            _nbemScorer = nbemScorer;
            _priceAnalyzer = priceAnalyzer;
            _textAnalyzer = textAnalyzer;
        }

        public List<Product> Search(IEnumerable<Product> products, SearchQuery query, NBEMData nbemData)
        {
            // Phase 1: Filter by text relevance (ignore price completely)
            var textFilteredProducts = products
                .Select(p => new ProductScore
                {
                    Product = p,
                    TextScore = _nbemScorer.CalculateTextOnlyRelevanceScore(p, query.QueryTerms, query.CleanedQuery, nbemData),
                    QueryCoverage = _textAnalyzer.CalculateQueryCoverage(p, query.QueryTerms, false)
                })
                .Where(x => x.TextScore >= SearchConstants.MIN_RELEVANCE_SCORE &&
                           x.QueryCoverage >= SearchConstants.QUERY_COVERAGE_THRESHOLD)
                .OrderByDescending(x => x.TextScore)
                .ThenByDescending(x => x.QueryCoverage)
                .Take(SearchConstants.MAX_RESULTS * 2) // Take more candidates for phase 2
                .ToList();

            if (!textFilteredProducts.Any())
            {
                // Fallback with lower threshold
                textFilteredProducts = products
                    .Select(p => new ProductScore
                    {
                        Product = p,
                        TextScore = _nbemScorer.CalculateTextOnlyRelevanceScore(p, query.QueryTerms, query.CleanedQuery, nbemData),
                        QueryCoverage = _textAnalyzer.CalculateQueryCoverage(p, query.QueryTerms, false)
                    })
                    .Where(x => x.TextScore > 0 && x.QueryCoverage >= SearchConstants.QUERY_COVERAGE_THRESHOLD * 0.5)
                    .OrderByDescending(x => x.TextScore)
                    .Take(SearchConstants.MAX_RESULTS)
                    .ToList();
            }

            if (!textFilteredProducts.Any())
                return new List<Product>();

            // Phase 2: Rank filtered products by price proximity
            var finalResults = textFilteredProducts
                .Select(x => new ProductScore
                {
                    Product = x.Product,
                    TextScore = x.TextScore,
                    PriceScore = _priceAnalyzer.CalculatePriceProximityScore(x.Product.Price, query.PriceQuery.Value),
                    CombinedScore = x.TextScore + _priceAnalyzer.CalculatePriceProximityScore(x.Product.Price, query.PriceQuery.Value) * 0.3
                })
                .OrderByDescending(x => x.CombinedScore)
                .ThenByDescending(x => x.TextScore)
                .Take(SearchConstants.MAX_RESULTS)
                .Select(x => x.Product)
                .ToList();

            return finalResults;
        }
    }
}
