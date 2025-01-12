using Microsoft.AspNetCore.Mvc;

namespace Boutique.Web.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Checkout()
        {
            return View();
        }
    }
}
