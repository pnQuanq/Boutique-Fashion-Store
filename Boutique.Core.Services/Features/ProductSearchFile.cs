//using AutoMapper;
//using Boutique.Core.Contracts.Product;
//using Boutique.Core.Domain.Entities;
//using Boutique.Core.Domain.Repositories;
//using Boutique.Core.Services.Abstractions.Features;
//using System.Text.RegularExpressions;

//public class ProductSearchFile : IProductSearchService
//{
//    private readonly IProductRepository _productRepository;
//    private readonly IMapper _mapper;

//    // Cấu trúc dữ liệu cho NBEM
//    private Dictionary<string, Dictionary<string, double>> _termCategoryProbabilities = new();
//    private Dictionary<string, double> _categoryPriors = new();
//    private Dictionary<string, Tuple<double, double>> _priceParameters = new();
//    private Dictionary<string, double> _termIdf = new();
//    private Dictionary<string, int> _categoryDocCounts = new();
//    private HashSet<string> _stopWords = new();
//    private int _totalDocuments = 0;
//    private bool _isInitialized = false;

//    // Threshold cho accuracy
//    private const double MIN_RELEVANCE_SCORE = 0.15; // Tăng threshold
//    private const int MAX_RESULTS = 15; // Giảm số lượng kết quả
//    private const double EXACT_MATCH_BONUS = 3.0;
//    private const double PARTIAL_MATCH_BONUS = 1.8;
//    private const double PREFIX_MATCH_BONUS = 1.5;
//    private const double MIN_TERM_LENGTH = 2; // Minimum term length
//    private const double QUERY_COVERAGE_THRESHOLD = 0.4; // Giảm threshold để linh hoạt hơn khi có price

//    public ProductSearchFile(IProductRepository productRepository, IMapper mapper)
//    {
//        _productRepository = productRepository;
//        _mapper = mapper;
//        InitializeStopWords();
//    }

//    private void InitializeStopWords()
//    {
//        // English stop words for fashion context - thêm "price" vào stop words để loại bỏ khi extract terms
//        _stopWords = new HashSet<string>
//        {
//            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with",
//            "by", "from", "up", "about", "into", "through", "during", "before", "after",
//            "above", "below", "between", "among", "is", "are", "was", "were", "be", "been",
//            "have", "has", "had", "do", "does", "did", "will", "would", "could", "should",
//            "may", "might", "must", "can", "this", "that", "these", "those", "i", "you",
//            "he", "she", "it", "we", "they", "me", "him", "her", "us", "them", "my", "your",
//            "his", "hers", "its", "our", "their", "price", "cost", "vnd", "dong", "dollar", "$"
//        };
//    }

//    public async Task InitializeAutoTFSWeightsAsync()
//    {
//        var products = await _productRepository.GetAllWithCategoryAsync();
//        _totalDocuments = products.Count();

//        if (_totalDocuments == 0)
//        {
//            _isInitialized = false;
//            return;
//        }

//        // Reset all data structures
//        _termCategoryProbabilities.Clear();
//        _categoryPriors.Clear();
//        _priceParameters.Clear();
//        _termIdf.Clear();
//        _categoryDocCounts.Clear();

//        var categories = products
//            .Where(p => p.Category != null)
//            .Select(p => p.Category.Name)
//            .Distinct()
//            .ToList();

//        // Initialize data structures
//        foreach (var category in categories)
//        {
//            _termCategoryProbabilities[category] = new Dictionary<string, double>();
//            _categoryDocCounts[category] = products.Count(p => p.Category?.Name == category);
//            _categoryPriors[category] = (double)_categoryDocCounts[category] / _totalDocuments;
//        }

//        // Collect terms and calculate frequencies
//        var allTerms = new HashSet<string>();
//        var documentFrequency = new Dictionary<string, int>();
//        var termCategoryCounts = new Dictionary<string, Dictionary<string, int>>();

//        foreach (var product in products)
//        {
//            var terms = ExtractAndNormalizeTerms(product);
//            var uniqueTermsInDoc = new HashSet<string>(terms);

//            foreach (var term in uniqueTermsInDoc)
//            {
//                allTerms.Add(term);

//                if (!documentFrequency.ContainsKey(term))
//                    documentFrequency[term] = 0;
//                documentFrequency[term]++;

//                if (product.Category != null)
//                {
//                    var category = product.Category.Name;
//                    if (!termCategoryCounts.ContainsKey(term))
//                        termCategoryCounts[term] = new Dictionary<string, int>();

//                    if (!termCategoryCounts[term].ContainsKey(category))
//                        termCategoryCounts[term][category] = 0;

//                    termCategoryCounts[term][category]++;
//                }
//            }
//        }

//        // Calculate IDF with better discrimination
//        foreach (var term in allTerms)
//        {
//            double idf = Math.Log((double)_totalDocuments / (1 + documentFrequency[term]));
//            // Boost IDF for rare terms (better discrimination)
//            if (documentFrequency[term] <= 3) idf *= 1.5;
//            _termIdf[term] = idf;
//        }

//        // Calculate P(term|category) with enhanced smoothing
//        var vocabularySize = allTerms.Count;
//        foreach (var category in categories)
//        {
//            foreach (var term in allTerms)
//            {
//                int termCountInCategory = 0;
//                if (termCategoryCounts.ContainsKey(term) &&
//                    termCategoryCounts[term].ContainsKey(category))
//                {
//                    termCountInCategory = termCategoryCounts[term][category];
//                }

//                // Enhanced Laplace smoothing for better accuracy
//                double smoothingFactor = documentFrequency[term] > 5 ? 0.5 : 1.0;
//                _termCategoryProbabilities[category][term] =
//                    (double)(termCountInCategory + smoothingFactor) / (_categoryDocCounts[category] + vocabularySize * smoothingFactor);
//            }
//        }

//        // Calculate Gaussian parameters for price
//        foreach (var category in categories)
//        {
//            var pricesInCategory = products
//                .Where(p => p.Category?.Name == category)
//                .Select(p => (double)p.Price)
//                .ToList();

//            if (pricesInCategory.Any())
//            {
//                double mean = pricesInCategory.Average();
//                double variance = pricesInCategory.Sum(p => Math.Pow(p - mean, 2)) / pricesInCategory.Count;
//                double stdDev = Math.Sqrt(Math.Max(variance, 1.0));
//                _priceParameters[category] = new Tuple<double, double>(mean, stdDev);
//            }
//            else
//            {
//                _priceParameters[category] = new Tuple<double, double>(0, 1);
//            }
//        }

//        _isInitialized = true;
//    }

//    private List<string> ExtractAndNormalizeTerms(Product product)
//    {
//        var text = $"{product.Name} {product.Category?.Name}".ToLower();

//        // Remove special characters but keep spaces and hyphens
//        text = Regex.Replace(text, @"[^\w\s\-]", " ");

//        var terms = text.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
//                       .Where(term => term.Length >= MIN_TERM_LENGTH && !_stopWords.Contains(term))
//                       .Where(term => !IsNumericOnly(term)) // Filter out pure numbers
//                       .Select(term => StemWord(term)) // Basic stemming
//                       .Distinct()
//                       .ToList();

//        return terms;
//    }

//    private bool IsNumericOnly(string term)
//    {
//        return decimal.TryParse(term, out _);
//    }

//    private string StemWord(string word)
//    {
//        // Basic English stemming for common fashion terms
//        if (word.EndsWith("ing") && word.Length > 5)
//            return word.Substring(0, word.Length - 3);
//        if (word.EndsWith("ed") && word.Length > 4)
//            return word.Substring(0, word.Length - 2);
//        if (word.EndsWith("s") && word.Length > 3 && !word.EndsWith("ss"))
//            return word.Substring(0, word.Length - 1);

//        return word;
//    }

//    public async Task<List<ProductDto>> SearchProductsByNameAsync(string searchString)
//    {
//        if (string.IsNullOrWhiteSpace(searchString))
//            throw new ArgumentException("Search string cannot be empty.", nameof(searchString));

//        if (!_isInitialized)
//            await InitializeAutoTFSWeightsAsync();

//        var allProducts = await _productRepository.GetAllWithCategoryAsync();

//        if (!allProducts.Any())
//            throw new Exception("No products available.");

//        var priceQuery = ExtractPriceFromQuery(searchString);
//        var cleanedQuery = RemovePriceFromQuery(searchString, priceQuery);
//        var queryTerms = ExtractQueryTerms(cleanedQuery);

//        if (!queryTerms.Any() && !priceQuery.HasValue)
//            throw new Exception("No valid search terms found.");

//        List<Product> finalResults;

//        if (priceQuery.HasValue && queryTerms.Any())
//        {
//            // Two-phase filtering: Text first, then price ranking
//            finalResults = SearchWithTwoPhasesFiltering(allProducts, queryTerms, priceQuery.Value, cleanedQuery);
//        }
//        else if (queryTerms.Any())
//        {
//            // Only text search
//            finalResults = SearchByTextOnly(allProducts, queryTerms, cleanedQuery);
//        }
//        else
//        {
//            // Only price search
//            finalResults = SearchByPriceOnly(allProducts, priceQuery.Value);
//        }

//        if (!finalResults.Any())
//            throw new Exception($"No relevant products found for '{searchString}'.");

//        return _mapper.Map<List<ProductDto>>(finalResults);
//    }

//    private List<Product> SearchWithTwoPhasesFiltering(IEnumerable<Product> allProducts, List<string> queryTerms, decimal priceQuery, string cleanedQuery)
//    {
//        // Phase 1: Filter by text relevance (ignore price completely)
//        var textFilteredProducts = allProducts
//            .Select(p => new
//            {
//                Product = p,
//                TextScore = CalculateTextOnlyRelevanceScore(p, queryTerms, cleanedQuery),
//                QueryCoverage = CalculateQueryCoverage(p, queryTerms, false)
//            })
//            .Where(x => x.TextScore >= MIN_RELEVANCE_SCORE && x.QueryCoverage >= QUERY_COVERAGE_THRESHOLD)
//            .OrderByDescending(x => x.TextScore)
//            .ThenByDescending(x => x.QueryCoverage)
//            .Take(MAX_RESULTS * 2) // Take more candidates for phase 2
//            .ToList();

//        if (!textFilteredProducts.Any())
//        {
//            // Fallback with lower threshold
//            textFilteredProducts = allProducts
//                .Select(p => new
//                {
//                    Product = p,
//                    TextScore = CalculateTextOnlyRelevanceScore(p, queryTerms, cleanedQuery),
//                    QueryCoverage = CalculateQueryCoverage(p, queryTerms, false)
//                })
//                .Where(x => x.TextScore > 0 && x.QueryCoverage >= QUERY_COVERAGE_THRESHOLD * 0.5)
//                .OrderByDescending(x => x.TextScore)
//                .Take(MAX_RESULTS)
//                .ToList();
//        }

//        if (!textFilteredProducts.Any())
//            return new List<Product>();

//        // Phase 2: Rank filtered products by price proximity
//        var finalResults = textFilteredProducts
//            .Select(x => new
//            {
//                Product = x.Product,
//                TextScore = x.TextScore,
//                PriceScore = CalculatePriceProximityScore(x.Product.Price, priceQuery),
//                CombinedScore = x.TextScore + CalculatePriceProximityScore(x.Product.Price, priceQuery) * 0.3 // Price has lower weight
//            })
//            .OrderByDescending(x => x.CombinedScore)
//            .ThenByDescending(x => x.TextScore) // Secondary sort by text relevance
//            .Take(MAX_RESULTS)
//            .Select(x => x.Product)
//            .ToList();

//        return finalResults;
//    }

//    private List<Product> SearchByTextOnly(IEnumerable<Product> allProducts, List<string> queryTerms, string cleanedQuery)
//    {
//        var scoredProducts = allProducts
//            .Select(p => new
//            {
//                Product = p,
//                Score = CalculateTextOnlyRelevanceScore(p, queryTerms, cleanedQuery),
//                QueryCoverage = CalculateQueryCoverage(p, queryTerms, false)
//            })
//            .Where(x => x.Score >= MIN_RELEVANCE_SCORE && x.QueryCoverage >= QUERY_COVERAGE_THRESHOLD)
//            .OrderByDescending(x => x.Score)
//            .ThenByDescending(x => x.QueryCoverage)
//            .Take(MAX_RESULTS)
//            .Select(x => x.Product)
//            .ToList();

//        if (!scoredProducts.Any())
//        {
//            // Fallback
//            scoredProducts = allProducts
//                .Select(p => new
//                {
//                    Product = p,
//                    Score = CalculateTextOnlyRelevanceScore(p, queryTerms, cleanedQuery)
//                })
//                .Where(x => x.Score > 0)
//                .OrderByDescending(x => x.Score)
//                .Take(MAX_RESULTS)
//                .Select(x => x.Product)
//                .ToList();
//        }

//        return scoredProducts;
//    }

//    private List<Product> SearchByPriceOnly(IEnumerable<Product> allProducts, decimal priceQuery)
//    {
//        return allProducts
//            .Select(p => new
//            {
//                Product = p,
//                PriceScore = CalculatePriceProximityScore(p.Price, priceQuery)
//            })
//            .OrderByDescending(x => x.PriceScore)
//            .Take(MAX_RESULTS)
//            .Select(x => x.Product)
//            .ToList();
//    }

//    private List<string> ExtractQueryTerms(string query)
//    {
//        var cleanQuery = Regex.Replace(query.ToLower(), @"[^\w\s\-]", " ");

//        return cleanQuery.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
//                        .Where(term => term.Length >= MIN_TERM_LENGTH && !_stopWords.Contains(term))
//                        .Where(term => !IsNumericOnly(term))
//                        .Select(term => StemWord(term))
//                        .Distinct()
//                        .ToList();
//    }

//    private decimal? ExtractPriceFromQuery(string query)
//    {
//        // Pattern 1: "price 500000", "cost 500000"
//        var pricePattern = @"\b(?:price|cost|giá|gia)\s+(\d+(?:\.\d+)?)\b";
//        var match = Regex.Match(query.ToLower(), pricePattern);
//        if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal price1))
//        {
//            return price1;
//        }

//        // Pattern 2: "$500", "500$", "500 vnd", "500 dong"
//        var currencyPattern = @"\b(?:\$(\d+(?:\.\d+)?)|(\d+(?:\.\d+)?)\$|(\d+(?:\.\d+)?)\s*(?:vnd|dong|đ))\b";
//        match = Regex.Match(query.ToLower(), currencyPattern);
//        if (match.Success)
//        {
//            var priceStr = match.Groups[1].Success ? match.Groups[1].Value :
//                          match.Groups[2].Success ? match.Groups[2].Value :
//                          match.Groups[3].Value;
//            if (decimal.TryParse(priceStr, out decimal price2))
//            {
//                return price2;
//            }
//        }

//        // Pattern 3: Standalone numbers (fallback)
//        var numberPattern = @"\b(\d{4,})\b"; // Numbers with at least 4 digits
//        match = Regex.Match(query, numberPattern);
//        if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal price3) && price3 >= 1000)
//        {
//            return price3;
//        }

//        return null;
//    }

//    private string RemovePriceFromQuery(string query, decimal? extractedPrice)
//    {
//        if (!extractedPrice.HasValue) return query;

//        var priceValue = extractedPrice.Value.ToString();

//        // Remove price patterns
//        var patterns = new[]
//        {
//            @"\b(?:price|cost|giá|gia)\s+\d+(?:\.\d+)?\b",
//            @"\b\$\d+(?:\.\d+)?\b",
//            @"\b\d+(?:\.\d+)?\$\b",
//            @"\b\d+(?:\.\d+)?\s*(?:vnd|dong|đ)\b",
//            @"\b" + Regex.Escape(priceValue) + @"\b"
//        };

//        var cleanedQuery = query;
//        foreach (var pattern in patterns)
//        {
//            cleanedQuery = Regex.Replace(cleanedQuery, pattern, " ", RegexOptions.IgnoreCase);
//        }

//        // Clean up extra spaces
//        cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ").Trim();

//        return cleanedQuery;
//    }

//    private double CalculateTextOnlyRelevanceScore(Product product, List<string> queryTerms, string originalQuery)
//    {
//        if (product?.Category == null || !_isInitialized)
//            return CalculateBasicTextScore(product, queryTerms, originalQuery);

//        var category = product.Category.Name;

//        if (!_categoryPriors.ContainsKey(category))
//            return CalculateBasicTextScore(product, queryTerms, originalQuery);

//        double score = 0.0;
//        var productTerms = ExtractAndNormalizeTerms(product);

//        // Exact match bonus
//        if (!string.IsNullOrWhiteSpace(originalQuery) && product.Name.ToLower().Contains(originalQuery.ToLower()))
//        {
//            score += EXACT_MATCH_BONUS;
//        }

//        // Term-by-term scoring with NBEM
//        foreach (var queryTerm in queryTerms)
//        {
//            double termScore = 0.0;

//            // Check for exact matches
//            if (productTerms.Contains(queryTerm))
//            {
//                termScore += EXACT_MATCH_BONUS;
//            }
//            // Check for partial matches
//            else if (productTerms.Any(pt => pt.Contains(queryTerm) || queryTerm.Contains(pt)))
//            {
//                termScore += PARTIAL_MATCH_BONUS;
//            }
//            // Check for fuzzy matches
//            else if (productTerms.Any(pt => LevenshteinDistance(pt, queryTerm) <= 1))
//            {
//                termScore += 1.0;
//            }

//            // Apply NBEM probability if term exists in training data
//            if (_termCategoryProbabilities[category].ContainsKey(queryTerm))
//            {
//                double termProb = _termCategoryProbabilities[category][queryTerm];
//                double idf = _termIdf.TryGetValue(queryTerm, out var idfValue) ? idfValue : 1.0;
//                termScore *= (termProb * idf);
//            }

//            score += termScore;
//        }

//        // Category prior
//        score *= _categoryPriors[category];

//        return Math.Max(score, 0.0);
//    }

//    private double CalculateBasicTextScore(Product product, List<string> queryTerms, string originalQuery)
//    {
//        if (product == null) return 0.0;

//        double score = 0.0;
//        var productText = $"{product.Name} {product.Category?.Name}".ToLower();

//        // Exact phrase match gets highest score
//        if (!string.IsNullOrWhiteSpace(originalQuery) && productText.Contains(originalQuery.ToLower()))
//        {
//            score += EXACT_MATCH_BONUS * 2;
//        }

//        // Individual term matches
//        foreach (var term in queryTerms)
//        {
//            if (product.Name.ToLower().Contains(term))
//            {
//                score += EXACT_MATCH_BONUS;
//            }
//            else if (product.Category?.Name.ToLower().Contains(term) == true)
//            {
//                score += PARTIAL_MATCH_BONUS;
//            }
//        }

//        return score;
//    }

//    private double CalculatePriceProximityScore(decimal productPrice, decimal targetPrice)
//    {
//        double priceDiff = Math.Abs((double)productPrice - (double)targetPrice) / (double)targetPrice;

//        // Scoring based on percentage difference
//        if (priceDiff <= 0.05) // Within 5%
//            return 5.0;
//        else if (priceDiff <= 0.1) // Within 10%
//            return 4.0;
//        else if (priceDiff <= 0.15) // Within 15%
//            return 3.0;
//        else if (priceDiff <= 0.25) // Within 25%
//            return 2.0;
//        else if (priceDiff <= 0.5) // Within 50%
//            return 1.0;
//        else if (priceDiff <= 1.0) // Within 100%
//            return 0.5;
//        else
//            return 0.1; // Very far from target price
//    }

//    private double CalculateQueryCoverage(Product product, List<string> queryTerms, bool hasPriceQuery)
//    {
//        if (!queryTerms.Any())
//        {
//            // Nếu chỉ có price query mà không có terms, coverage = 1
//            return hasPriceQuery ? 1.0 : 0.0;
//        }

//        var productTerms = ExtractAndNormalizeTerms(product);
//        var matchedTerms = queryTerms.Count(qt =>
//            productTerms.Any(pt => pt.Contains(qt) || qt.Contains(pt) ||
//                            LevenshteinDistance(pt, qt) <= 1));

//        return (double)matchedTerms / queryTerms.Count;
//    }

//    private int LevenshteinDistance(string s1, string s2)
//    {
//        if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
//        if (string.IsNullOrEmpty(s2)) return s1.Length;

//        var distance = new int[s1.Length + 1, s2.Length + 1];

//        for (int i = 0; i <= s1.Length; i++) distance[i, 0] = i;
//        for (int j = 0; j <= s2.Length; j++) distance[0, j] = j;

//        for (int i = 1; i <= s1.Length; i++)
//        {
//            for (int j = 1; j <= s2.Length; j++)
//            {
//                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
//                distance[i, j] = Math.Min(Math.Min(
//                    distance[i - 1, j] + 1,
//                    distance[i, j - 1] + 1),
//                    distance[i - 1, j - 1] + cost);
//            }
//        }

//        return distance[s1.Length, s2.Length];
//    }

//    private double GaussianProbability(double x, double mean, double stdDev)
//    {
//        if (stdDev <= 0) stdDev = 1.0;

//        double exponent = -0.5 * Math.Pow((x - mean) / stdDev, 2);
//        double coefficient = 1.0 / (stdDev * Math.Sqrt(2 * Math.PI));

//        return coefficient * Math.Exp(exponent);
//    }
//}