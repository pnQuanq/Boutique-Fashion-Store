using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Domain.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<Cart> GetCartWithItemsByUserIdAsync(string userId);
        Task<CartItem> GetCartItemByIdAsync(int cartItemId);
        Task DeleteCartItemAsync(int cartItemId);
    }
}
