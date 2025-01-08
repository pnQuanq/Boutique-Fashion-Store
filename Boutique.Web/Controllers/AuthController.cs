using Microsoft.AspNetCore.Mvc;
using Boutique.Core.Contracts.Auth;
using Boutique.Core.Contracts.User;
using Boutique.Core.Services.Abstractions.Features;

namespace Boutique.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                await _authService.RegisterAsync(registerDto);
                TempData["SuccessMessage"] = "Account Created successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to create account: {ex.Message}";
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDto);

                Response.Cookies.Append("AccessToken", token, new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(2)
                });

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = $"Login failed: {ex.Message}";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Login");
            }
        }


    }
}
