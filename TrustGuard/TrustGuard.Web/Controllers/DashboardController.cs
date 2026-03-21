using Microsoft.AspNetCore.Mvc;

namespace TrustGuard.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
