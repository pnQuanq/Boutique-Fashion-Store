using System.Text.RegularExpressions;

namespace Boutique.Core.Services.Features.ProductSearchService.PriceProcessing
{
    public class PriceExtractor
    {
        public decimal? ExtractPriceFromQuery(string query)
        {
            // Pattern 1: "price 500000", "cost 500000"
            var pricePattern = @"\b(?:price|cost|giá|gia)\s+(\d+(?:\.\d+)?)\b";
            var match = Regex.Match(query.ToLower(), pricePattern);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal price1))
            {
                return price1;
            }

            // Pattern 2: "$500", "500$", "500 vnd", "500 dong"
            var currencyPattern = @"\b(?:\$(\d+(?:\.\d+)?)|(\d+(?:\.\d+)?)\$|(\d+(?:\.\d+)?)\s*(?:vnd|dong|đ))\b";
            match = Regex.Match(query.ToLower(), currencyPattern);
            if (match.Success)
            {
                var priceStr = match.Groups[1].Success ? match.Groups[1].Value :
                              match.Groups[2].Success ? match.Groups[2].Value :
                              match.Groups[3].Value;
                if (decimal.TryParse(priceStr, out decimal price2))
                {
                    return price2;
                }
            }

            // Pattern 3: Standalone numbers (fallback)
            var numberPattern = @"\b(\d{4,})\b"; // Numbers with at least 4 digits
            match = Regex.Match(query, numberPattern);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal price3) && price3 >= 1000)
            {
                return price3;
            }

            return null;
        }

        public string RemovePriceFromQuery(string query, decimal? extractedPrice)
        {
            if (!extractedPrice.HasValue) return query;

            var priceValue = extractedPrice.Value.ToString();

            // Remove price patterns
            var patterns = new[]
            {
                @"\b(?:price|cost|giá|gia)\s+\d+(?:\.\d+)?\b",
                @"\b\$\d+(?:\.\d+)?\b",
                @"\b\d+(?:\.\d+)?\$\b",
                @"\b\d+(?:\.\d+)?\s*(?:vnd|dong|đ)\b",
                @"\b" + Regex.Escape(priceValue) + @"\b"
            };

            var cleanedQuery = query;
            foreach (var pattern in patterns)
            {
                cleanedQuery = Regex.Replace(cleanedQuery, pattern, " ", RegexOptions.IgnoreCase);
            }

            // Clean up extra spaces
            cleanedQuery = Regex.Replace(cleanedQuery, @"\s+", " ").Trim();

            return cleanedQuery;
        }
    }
}
