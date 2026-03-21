using Microsoft.AspNetCore.Mvc;

namespace TrustGuard.Web.Controllers
{
    public class FinanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
