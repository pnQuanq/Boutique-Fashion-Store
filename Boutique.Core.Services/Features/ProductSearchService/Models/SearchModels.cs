using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Services.Features.ProductSearchService.Models
{
    public class SearchModels
    {
        public class SearchQuery
        {
            public string OriginalQuery { get; set; }
            public string CleanedQuery { get; set; }
            public List<string> QueryTerms { get; set; }
            public decimal? PriceQuery { get; set; }

            public SearchQuery()
            {
                QueryTerms = new List<string>();
            }
        }

        public class ProductScore
        {
            public Product Product { get; set; }
            public double TextScore { get; set; }
            public double PriceScore { get; set; }
            public double CombinedScore { get; set; }
            public double QueryCoverage { get; set; }
        }

        public class SearchConstants
        {
            public const double MIN_RELEVANCE_SCORE = 0.15;
            public const int MAX_RESULTS = 15;
            public const double EXACT_MATCH_BONUS = 3.0;
            public const double PARTIAL_MATCH_BONUS = 1.8;
            public const double PREFIX_MATCH_BONUS = 1.5;
            public const double MIN_TERM_LENGTH = 2;
            public const double QUERY_COVERAGE_THRESHOLD = 0.4;
        }
    }
}
