using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Features.ProductSearchService.Models;
using Boutique.Core.Services.Features.ProductSearchService.TextProcessing;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;

namespace Boutique.Core.Services.Features.ProductSearchService.NBEMEngine
{
    public class NBEMScorer
    {
        private readonly TextProcessor _textProcessor;
        private readonly TextAnalyzer _textAnalyzer;

        public NBEMScorer(TextProcessor textProcessor, TextAnalyzer textAnalyzer)
        {
            _textProcessor = textProcessor;
            _textAnalyzer = textAnalyzer;
        }

        public double CalculateTextOnlyRelevanceScore(Product product, List<string> queryTerms, string originalQuery, NBEMData nbemData)
        {
            if (product?.Category == null || !nbemData.IsInitialized)
                return _textAnalyzer.CalculateBasicTextScore(product, queryTerms, originalQuery);

            var category = product.Category.Name;

            if (!nbemData.CategoryPriors.ContainsKey(category))
                return _textAnalyzer.CalculateBasicTextScore(product, queryTerms, originalQuery);

            double score = 0.0;
            var productTerms = _textProcessor.ExtractAndNormalizeTerms(product);

            // Exact match bonus
            if (!string.IsNullOrWhiteSpace(originalQuery) && product.Name.ToLower().Contains(originalQuery.ToLower()))
            {
                score += SearchConstants.EXACT_MATCH_BONUS;
            }

            // Term-by-term scoring with NBEM
            foreach (var queryTerm in queryTerms)
            {
                double termScore = 0.0;

                // Check for exact matches
                if (productTerms.Contains(queryTerm))
                {
                    termScore += SearchConstants.EXACT_MATCH_BONUS;
                }
                // Check for partial matches
                else if (productTerms.Any(pt => pt.Contains(queryTerm) || queryTerm.Contains(pt)))
                {
                    termScore += SearchConstants.PARTIAL_MATCH_BONUS;
                }
                // Check for fuzzy matches
                else if (productTerms.Any(pt => _textAnalyzer.LevenshteinDistance(pt, queryTerm) <= 1))
                {
                    termScore += 1.0;
                }

                // Apply NBEM probability if term exists in training data
                if (nbemData.TermCategoryProbabilities[category].ContainsKey(queryTerm))
                {
                    double termProb = nbemData.TermCategoryProbabilities[category][queryTerm];
                    double idf = nbemData.TermIdf.TryGetValue(queryTerm, out var idfValue) ? idfValue : 1.0;
                    termScore *= (termProb * idf);
                }

                score += termScore;
            }

            // Category prior
            score *= nbemData.CategoryPriors[category];

            return Math.Max(score, 0.0);
        }
    }
}
