using Boutique.Core.Domain.Entities;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;

namespace Boutique.Core.Services.Features.ProductSearchService.TextProcessing
{
    public class TextAnalyzer
    {
        private readonly TextProcessor _textProcessor;

        public TextAnalyzer(TextProcessor textProcessor)
        {
            _textProcessor = textProcessor;
        }

        public double CalculateBasicTextScore(Product product, List<string> queryTerms, string originalQuery)
        {
            if (product == null) return 0.0;

            double score = 0.0;
            var productText = $"{product.Name} {product.Category?.Name}".ToLower();

            // Exact phrase match gets highest score
            if (!string.IsNullOrWhiteSpace(originalQuery) && productText.Contains(originalQuery.ToLower()))
            {
                score += SearchConstants.EXACT_MATCH_BONUS * 2;
            }

            // Individual term matches
            foreach (var term in queryTerms)
            {
                if (product.Name.ToLower().Contains(term))
                {
                    score += SearchConstants.EXACT_MATCH_BONUS;
                }
                else if (product.Category?.Name.ToLower().Contains(term) == true)
                {
                    score += SearchConstants.PARTIAL_MATCH_BONUS;
                }
            }

            return score;
        }

        public double CalculateQueryCoverage(Product product, List<string> queryTerms, bool hasPriceQuery)
        {
            if (!queryTerms.Any())
            {
                return hasPriceQuery ? 1.0 : 0.0;
            }

            var productTerms = _textProcessor.ExtractAndNormalizeTerms(product);
            var matchedTerms = queryTerms.Count(qt =>
                productTerms.Any(pt => pt.Contains(qt) || qt.Contains(pt) ||
                                LevenshteinDistance(pt, qt) <= 1));

            return (double)matchedTerms / queryTerms.Count;
        }

        public int LevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            var distance = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++) distance[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) distance[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(
                        distance[i - 1, j] + 1,
                        distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[s1.Length, s2.Length];
        }
    }
}
