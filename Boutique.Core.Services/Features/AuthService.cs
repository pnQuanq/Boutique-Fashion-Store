using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using Boutique.Core.Contracts.Auth;
using Boutique.Core.Contracts.User;
using Boutique.Core.Domain.Entities;
using Boutique.Core.Services.Abstractions.Features;

namespace Boutique.Core.Services.Features
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }
        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            if (!IsValidEmail(registerDto.Email))
            {
                throw new ArgumentException("The email format is invalid.");
            }

            if (registerDto.Password != registerDto.RePassword)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                DateCreated = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            return result;
        }
        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                throw new UnauthorizedAccessException("Invalid login attempt.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid login attempt.");
            }

            // Generate JWT Token
            var token = await _tokenService.GenerateAccessTokenAsync(user);

            return token;
        }
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}
