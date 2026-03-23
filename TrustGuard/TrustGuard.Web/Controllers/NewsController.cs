using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustGuard.Application.Interfaces;

namespace TrustGuard.Web.Controllers
{
    public class NewsController : Controller
    {
        private readonly IMlService _mlService;
        private readonly INewsCheckService _newsCheckService; 

        public NewsController(IMlService mlService, INewsCheckService newsCheckService)
        {
            _mlService = mlService;
            _newsCheckService = newsCheckService;
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

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    await _newsCheckService.SaveCheckResultAsync(userId, textContent, result.Verdict!, result.ConfidenceScore);
                    ViewBag.Message = "Результат успішно збережено в історію!";
                }
                else
                {
                    ViewBag.Message = "Результат не збережено. Увійдіть у систему.";
                }
            }
            else
            {
                ViewBag.Error = "Не вдалося з'єднатися з сервером аналізу. Перевірте, чи запущений Python FastAPI.";
            }

            return View("Index");
        }
    }
}