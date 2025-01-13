using Boutique.Core.Contracts.Product;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IRecommendationService
    {
        Task<ProductRecommendationDto> GetRecommendationsForUserAsync(string userId);
    }
}
