namespace Boutique.Core.Services.Features.ProductSearchService.Models
{
    public class NBEMData
    {
        public Dictionary<string, Dictionary<string, double>> TermCategoryProbabilities { get; set; }
        public Dictionary<string, double> CategoryPriors { get; set; }
        public Dictionary<string, Tuple<double, double>> PriceParameters { get; set; }
        public Dictionary<string, double> TermIdf { get; set; }
        public Dictionary<string, int> CategoryDocCounts { get; set; }
        public int TotalDocuments { get; set; }
        public bool IsInitialized { get; set; }

        public NBEMData()
        {
            TermCategoryProbabilities = new Dictionary<string, Dictionary<string, double>>();
            CategoryPriors = new Dictionary<string, double>();
            PriceParameters = new Dictionary<string, Tuple<double, double>>();
            TermIdf = new Dictionary<string, double>();
            CategoryDocCounts = new Dictionary<string, int>();
            IsInitialized = false;
        }
    }
}
