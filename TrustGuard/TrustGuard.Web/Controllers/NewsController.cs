using Microsoft.AspNetCore.Mvc;

namespace TrustGuard.Web.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
