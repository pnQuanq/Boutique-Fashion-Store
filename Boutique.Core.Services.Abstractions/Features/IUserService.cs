using Boutique.Core.Contracts.User;
using Boutique.Core.Domain.Entities;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task<string> CreateUserAsync(CreateUserDto dto);
        Task<(bool Success, List<string> Errors)> UpdateUserAsync(string id, UpdateUserDto dto);
        Task<(bool Success, List<string> Errors)> DeleteUserAsync(string id);
        Task<(bool Success, List<string> Errors)> ChangeUserRolesAsync(string id, List<string> roles);
    }
}
