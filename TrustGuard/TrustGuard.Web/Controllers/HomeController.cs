using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustGuard.Application.Interfaces;
using TrustGuard.Application.Models;

namespace TrustGuard.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly INewsCheckService _newsCheckService;

        public HomeController(INewsCheckService newsCheckService)
        {
            _newsCheckService = newsCheckService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return View(new DashboardStatsDto());
            }

            var stats = await _newsCheckService.GetDashboardStatsAsync(userId);

            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            var fullHistory = await _newsCheckService.GetFullUserHistoryAsync(userId);

            return View(fullHistory);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index");

            var details = await _newsCheckService.GetCheckDetailsAsync(id, userId);

            if (details == null)
            {
                return RedirectToAction("Index");
            }

            return View(details);
        }
    }
}