using Microsoft.AspNetCore.Mvc;
using TrustGuard.Application.Interfaces;

namespace TrustGuard.Web.Controllers
{
    public class NewsController : Controller
    {
        private readonly IMlService _mlService;

        public NewsController(IMlService mlService)
        {
            _mlService = mlService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(string textContent)
        {
            if (string.IsNullOrWhiteSpace(textContent))
            {
                ViewBag.Error = "Будь ласка, введіть текст новини для перевірки.";
                return View("Index");
            }

            var result = await _mlService.AnalyzeContentAsync(textContent);

            if (result != null)
            {
                ViewBag.Verdict = result.Verdict;
                ViewBag.Confidence = result.ConfidenceScore;
            }
            else
            {
                ViewBag.Error = "Не вдалося з'єднатися з сервером аналізу. Перевірте, чи запущений Python FastAPI.";
            }

            return View("Index");
        }
    }
}
   
