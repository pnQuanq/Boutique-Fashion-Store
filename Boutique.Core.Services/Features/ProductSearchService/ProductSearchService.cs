using AutoMapper;
using Boutique.Core.Contracts.Product;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Domain.Repositories;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Core.Services.Features.ProductSearchService.Models;
using Boutique.Core.Services.Features.ProductSearchService.NBEMEngine;
using Boutique.Core.Services.Features.ProductSearchService.PriceProcessing;
using Boutique.Core.Services.Features.ProductSearchService.SearchStrategies;
using Boutique.Core.Services.Features.ProductSearchService.TextProcessing;
using static Boutique.Core.Services.Features.ProductSearchService.Models.SearchModels;

namespace Boutique.Core.Services.Features.ProductSearchService
{
    public class ProductSearchService : IProductSearchService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        // Components
        private readonly TextProcessor _textProcessor;
        private readonly TextAnalyzer _textAnalyzer;
        private readonly PriceExtractor _priceExtractor;
        private readonly PriceAnalyzer _priceAnalyzer;
        private readonly NBEMInitializer _nbemInitializer;
        private readonly NBEMScorer _nbemScorer;

        // Search Strategies
        private readonly TextOnlySearchStrategy _textOnlySearchStrategy;
        private readonly PriceOnlySearchStrategy _priceOnlySearchStrategy;
        private readonly CombinedSearchStrategy _combinedSearchStrategy;

        // NBEM Data
        private NBEMData _nbemData;

        public ProductSearchService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;

            // Initialize components
            _textProcessor = new TextProcessor();
            _textAnalyzer = new TextAnalyzer(_textProcessor);
            _priceExtractor = new PriceExtractor();
            _priceAnalyzer = new PriceAnalyzer();
            _nbemInitializer = new NBEMInitializer(_textProcessor);
            _nbemScorer = new NBEMScorer(_textProcessor, _textAnalyzer);

            // Initialize strategies
            _textOnlySearchStrategy = new TextOnlySearchStrategy(_nbemScorer, _textAnalyzer);
            _priceOnlySearchStrategy = new PriceOnlySearchStrategy(_priceAnalyzer);
            _combinedSearchStrategy = new CombinedSearchStrategy(_nbemScorer, _priceAnalyzer, _textAnalyzer);

            // Initialize NBEM data
            _nbemData = new NBEMData();
        }

        public async Task InitializeAutoTFSWeightsAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            _nbemData = _nbemInitializer.InitializeNBEMData(products);
        }

        public async Task<List<ProductDto>> SearchProductsByNameAsync(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                throw new ArgumentException("Search string cannot be empty.", nameof(searchString));

            if (!_nbemData.IsInitialized)
                await InitializeAutoTFSWeightsAsync();

            var allProducts = await _productRepository.GetAllWithCategoryAsync();

            if (!allProducts.Any())
                throw new Exception("No products available.");

            // Parse the search query
            var searchQuery = ParseSearchQuery(searchString);

            if (!searchQuery.QueryTerms.Any() && !searchQuery.PriceQuery.HasValue)
                throw new Exception("No valid search terms found.");

            // Execute appropriate search strategy
            List<Product> finalResults = ExecuteSearchStrategy(allProducts, searchQuery);

            if (!finalResults.Any())
                throw new Exception($"No relevant products found for '{searchString}'.");

            return _mapper.Map<List<ProductDto>>(finalResults);
        }

        private SearchQuery ParseSearchQuery(string searchString)
        {
            var priceQuery = _priceExtractor.ExtractPriceFromQuery(searchString);
            var cleanedQuery = _priceExtractor.RemovePriceFromQuery(searchString, priceQuery);
            var queryTerms = _textProcessor.ExtractQueryTerms(cleanedQuery);

            return new SearchQuery
            {
                OriginalQuery = searchString,
                CleanedQuery = cleanedQuery,
                QueryTerms = queryTerms,
                PriceQuery = priceQuery
            };
        }

        private List<Product> ExecuteSearchStrategy(IEnumerable<Product> allProducts, SearchQuery searchQuery)
        {
            if (searchQuery.PriceQuery.HasValue && searchQuery.QueryTerms.Any())
            {
                // Combined search: text + price
                return _combinedSearchStrategy.Search(allProducts, searchQuery, _nbemData);
            }
            else if (searchQuery.QueryTerms.Any())
            {
                // Text-only search
                return _textOnlySearchStrategy.Search(allProducts, searchQuery, _nbemData);
            }
            else if (searchQuery.PriceQuery.HasValue)
            {
                // Price-only search
                return _priceOnlySearchStrategy.Search(allProducts, searchQuery.PriceQuery.Value);
            }
            else
            {
                return new List<Product>();
            }
        }
    }
}
