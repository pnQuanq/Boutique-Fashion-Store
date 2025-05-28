using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Features.ProductSearchService.Models;
using Boutique.Core.Services.Features.ProductSearchService.NBEMEngine;
using Boutique.Core.Services.Features.ProductSearchService.TextProcessing;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;

namespace Boutique.Core.Services.Features.ProductSearchService.SearchStrategies
{
    public class TextOnlySearchStrategy
    {
        private readonly NBEMScorer _nbemScorer;
        private readonly TextAnalyzer _textAnalyzer;

        public TextOnlySearchStrategy(NBEMScorer nbemScorer, TextAnalyzer textAnalyzer)
        {
            _nbemScorer = nbemScorer;
            _textAnalyzer = textAnalyzer;
        }

        public List<Product> Search(IEnumerable<Product> products, SearchQuery query, NBEMData nbemData)
        {
            var scoredProducts = products
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
                .Take(SearchConstants.MAX_RESULTS)
                .Select(x => x.Product)
                .ToList();

            if (!scoredProducts.Any())
            {
                // Fallback
                scoredProducts = products
                    .Select(p => new ProductScore
                    {
                        Product = p,
                        TextScore = _nbemScorer.CalculateTextOnlyRelevanceScore(p, query.QueryTerms, query.CleanedQuery, nbemData)
                    })
                    .Where(x => x.TextScore > 0)
                    .OrderByDescending(x => x.TextScore)
                    .Take(SearchConstants.MAX_RESULTS)
                    .Select(x => x.Product)
                    .ToList();
            }

            return scoredProducts;
        }
    }
}
