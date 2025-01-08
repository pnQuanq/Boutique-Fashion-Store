using Microsoft.AspNetCore.Identity;
using Boutique.Core.Contracts.Auth;
using Boutique.Core.Contracts.User;

namespace Boutique.Core.Services.Abstractions.Features
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
    }
}
