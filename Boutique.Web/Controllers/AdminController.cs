using Boutique.Core.Contracts.Order;
using Boutique.Core.Contracts.User;
using Boutique.Core.Services.Abstractions.Features;
using Boutique.Web.ViewModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boutique.Web.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        public AdminController(IOrderService orderService, IUserService userService) 
        { 
            _orderService = orderService;
            _userService = userService;
        }
        public IActionResult DashBoard()
        {
            return View();
        }
        public async Task<IActionResult> AdminTrackOrder()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var model = new OrderManagementViewModel();
            model.Orders = orders;

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromForm] UpdateOrderStatusDto dto)
        {
            await _orderService.UpdateOrderStatusAsync(dto);
            return RedirectToAction("AdminTrackOrder");
        }
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userService.GetAllUsersAsync();

            var model = new UserViewModel();
            model.Users = users;
            return View(model);
            
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserRoles(UpdateUserDto dto)
        {
            var user = await _userService.UpdateUserAsync(dto.UserId, dto);
            return RedirectToAction("UserManagement");
        }
    }
}
