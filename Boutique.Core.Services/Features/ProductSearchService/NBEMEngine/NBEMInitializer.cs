using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Features.ProductSearchService.Models;
using Boutique.Core.Services.Features.ProductSearchService.TextProcessing;

namespace Boutique.Core.Services.Features.ProductSearchService.NBEMEngine
{
    public class NBEMInitializer
    {
        private readonly TextProcessor _textProcessor;

        public NBEMInitializer(TextProcessor textProcessor)
        {
            _textProcessor = textProcessor;
        }

        public NBEMData InitializeNBEMData(IEnumerable<Product> products)
        {
            var nbemData = new NBEMData();
            nbemData.TotalDocuments = products.Count();

            if (nbemData.TotalDocuments == 0)
            {
                nbemData.IsInitialized = false;
                return nbemData;
            }

            var categories = products
                .Where(p => p.Category != null)
                .Select(p => p.Category.Name)
                .Distinct()
                .ToList();

            // Initialize data structures
            foreach (var category in categories)
            {
                nbemData.TermCategoryProbabilities[category] = new Dictionary<string, double>();
                nbemData.CategoryDocCounts[category] = products.Count(p => p.Category?.Name == category);
                nbemData.CategoryPriors[category] = (double)nbemData.CategoryDocCounts[category] / nbemData.TotalDocuments;
            }

            // Collect terms and calculate frequencies
            var allTerms = new HashSet<string>();
            var documentFrequency = new Dictionary<string, int>();
            var termCategoryCounts = new Dictionary<string, Dictionary<string, int>>();

            foreach (var product in products)
            {
                var terms = _textProcessor.ExtractAndNormalizeTerms(product);
                var uniqueTermsInDoc = new HashSet<string>(terms);

                foreach (var term in uniqueTermsInDoc)
                {
                    allTerms.Add(term);

                    if (!documentFrequency.ContainsKey(term))
                        documentFrequency[term] = 0;
                    documentFrequency[term]++;

                    if (product.Category != null)
                    {
                        var category = product.Category.Name;
                        if (!termCategoryCounts.ContainsKey(term))
                            termCategoryCounts[term] = new Dictionary<string, int>();

                        if (!termCategoryCounts[term].ContainsKey(category))
                            termCategoryCounts[term][category] = 0;

                        termCategoryCounts[term][category]++;
                    }
                }
            }

            // Calculate IDF with better discrimination
            foreach (var term in allTerms)
            {
                double idf = Math.Log((double)nbemData.TotalDocuments / (1 + documentFrequency[term]));
                if (documentFrequency[term] <= 3) idf *= 1.5;
                nbemData.TermIdf[term] = idf;
            }

            // Calculate P(term|category) with enhanced smoothing
            var vocabularySize = allTerms.Count;
            foreach (var category in categories)
            {
                foreach (var term in allTerms)
                {
                    int termCountInCategory = 0;
                    if (termCategoryCounts.ContainsKey(term) &&
                        termCategoryCounts[term].ContainsKey(category))
                    {
                        termCountInCategory = termCategoryCounts[term][category];
                    }

                    double smoothingFactor = documentFrequency[term] > 5 ? 0.5 : 1.0;
                    nbemData.TermCategoryProbabilities[category][term] =
                        (double)(termCountInCategory + smoothingFactor) / (nbemData.CategoryDocCounts[category] + vocabularySize * smoothingFactor);
                }
            }

            // Calculate Gaussian parameters for price
            foreach (var category in categories)
            {
                var pricesInCategory = products
                    .Where(p => p.Category?.Name == category)
                    .Select(p => (double)p.Price)
                    .ToList();

                if (pricesInCategory.Any())
                {
                    double mean = pricesInCategory.Average();
                    double variance = pricesInCategory.Sum(p => Math.Pow(p - mean, 2)) / pricesInCategory.Count;
                    double stdDev = Math.Sqrt(Math.Max(variance, 1.0));
                    nbemData.PriceParameters[category] = new Tuple<double, double>(mean, stdDev);
                }
                else
                {
                    nbemData.PriceParameters[category] = new Tuple<double, double>(0, 1);
                }
            }

            nbemData.IsInitialized = true;
            return nbemData;
        }
    }
}
