using Boutique.Core.Domain.Entities;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;
using System.Text.RegularExpressions;

namespace Boutique.Core.Services.Features.ProductSearchService.TextProcessing
{
    public class TextProcessor
    {
        private readonly HashSet<string> _stopWords;

        public TextProcessor()
        {
            _stopWords = new HashSet<string>
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with",
                "by", "from", "up", "about", "into", "through", "during", "before", "after",
                "above", "below", "between", "among", "is", "are", "was", "were", "be", "been",
                "have", "has", "had", "do", "does", "did", "will", "would", "could", "should",
                "may", "might", "must", "can", "this", "that", "these", "those", "i", "you",
                "he", "she", "it", "we", "they", "me", "him", "her", "us", "them", "my", "your",
                "his", "hers", "its", "our", "their", "price", "cost", "vnd", "dong", "dollar", "$"
            };
        }
        public List<string> ExtractAndNormalizeTerms(Product product)
        {
            var text = $"{product.Name} {product.Category?.Name}".ToLower();

            // Remove special characters but keep spaces and hyphens
            text = Regex.Replace(text, @"[^\w\s\-]", " ");

            var terms = text.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                           .Where(term => term.Length >= SearchConstants.MIN_TERM_LENGTH && !_stopWords.Contains(term))
                           .Where(term => !IsNumericOnly(term))
                           .Select(term => StemWord(term))
                           .Distinct()
                           .ToList();

            return terms;
        }
        public List<string> ExtractQueryTerms(string query)
        {
            var cleanQuery = Regex.Replace(query.ToLower(), @"[^\w\s\-]", " ");

            return cleanQuery.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(term => term.Length >= SearchConstants.MIN_TERM_LENGTH && !_stopWords.Contains(term))
                            .Where(term => !IsNumericOnly(term))
                            .Select(term => StemWord(term))
                            .Distinct()
                            .ToList();
        }
        private bool IsNumericOnly(string term)
        {
            return decimal.TryParse(term, out _);
        }
        private string StemWord(string word)
        {
            // Basic English stemming for common fashion terms
            if (word.EndsWith("ing") && word.Length > 5)
                return word.Substring(0, word.Length - 3);
            if (word.EndsWith("ed") && word.Length > 4)
                return word.Substring(0, word.Length - 2);
            if (word.EndsWith("s") && word.Length > 3 && !word.EndsWith("ss"))
                return word.Substring(0, word.Length - 1);

            return word;
        }
    }
}
