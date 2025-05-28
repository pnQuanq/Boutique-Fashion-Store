namespace Boutique.Core.Services.Features.ProductSearchService.PriceProcessing
{
    public class PriceAnalyzer
    {
        public double CalculatePriceProximityScore(decimal productPrice, decimal targetPrice)
        {
            double priceDiff = Math.Abs((double)productPrice - (double)targetPrice) / (double)targetPrice;

            // Scoring based on percentage difference
            if (priceDiff <= 0.05) // Within 5%
                return 5.0;
            else if (priceDiff <= 0.1) // Within 10%
                return 4.0;
            else if (priceDiff <= 0.15) // Within 15%
                return 3.0;
            else if (priceDiff <= 0.25) // Within 25%
                return 2.0;
            else if (priceDiff <= 0.5) // Within 50%
                return 1.0;
            else if (priceDiff <= 1.0) // Within 100%
                return 0.5;
            else
                return 0.1; // Very far from target price
        }

        public double GaussianProbability(double x, double mean, double stdDev)
        {
            if (stdDev <= 0) stdDev = 1.0;

            double exponent = -0.5 * Math.Pow((x - mean) / stdDev, 2);
            double coefficient = 1.0 / (stdDev * Math.Sqrt(2 * Math.PI));

            return coefficient * Math.Exp(exponent);
        }
    }
}
