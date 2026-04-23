using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Web.Controllers
{
    public class NewsController : Controller
    {
        private readonly IMlService _mlService;
        private readonly INewsCheckService _newsCheckService;
        private readonly IFileParserService _fileParserService;
        private readonly IUrlParserService _urlParserService; // ДОДАЛИ НОВИЙ СЕРВІС

        // ОНОВИЛИ КОНСТРУКТОР
        public NewsController(
            IMlService mlService,
            INewsCheckService newsCheckService,
            IFileParserService fileParserService,
            IUrlParserService urlParserService)
        {
            _mlService = mlService;
            _newsCheckService = newsCheckService;
            _fileParserService = fileParserService;
            _urlParserService = urlParserService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(string? textContent, string? urlContent, IFormFile? documentFile, IFormFile? imageFile)
        {
            string textToAnalyze = "";
            ContentType detectedType = ContentType.Text;
            MlAnalysisResponse? result = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(textContent))
                {
                    detectedType = ContentType.Text;
                    textToAnalyze = textContent;
                }
                // --- ОНОВЛЕНИЙ БЛОК ДЛЯ URL ---
                else if (!string.IsNullOrWhiteSpace(urlContent))
                {
                    detectedType = ContentType.Url;
                    // Викликаємо наш парсер, щоб він скачав статтю!
                    textToAnalyze = await _urlParserService.ExtractTextFromUrlAsync(urlContent);

                    if (string.IsNullOrWhiteSpace(textToAnalyze) || textToAnalyze.StartsWith("Не вдалося"))
                    {
                        ViewBag.Error = "Не вдалося витягнути текст за цим посиланням. Можливо, сайт блокує парсинг або там немає статті.";
                        return View("Index");
                    }
                }
                // -------------------------------
                else if (documentFile != null && documentFile.Length > 0)
                {
                    detectedType = ContentType.Document;

                    using var stream = documentFile.OpenReadStream();
                    textToAnalyze = await _fileParserService.ExtractTextAsync(stream, documentFile.FileName);

                    if (string.IsNullOrWhiteSpace(textToAnalyze))
                    {
                        ViewBag.Error = "Не вдалося витягнути текст із файлу. Можливо, він порожній або це відскановані картинки.";
                        return View("Index");
                    }
                }
                else if (imageFile != null && imageFile.Length > 0)
                {
                    detectedType = ContentType.Document; // Або ContentType.Image, якщо ви додали його в Enum

                    using var stream = imageFile.OpenReadStream();
                    textToAnalyze = await _fileParserService.ExtractTextAsync(stream, imageFile.FileName);

                    if (string.IsNullOrWhiteSpace(textToAnalyze))
                    {
                        ViewBag.Error = "Не вдалося розпізнати текст на зображенні. Можливо, текст занадто розмитий.";
                        return View("Index");
                    }
                }
                else
                {
                    ViewBag.Error = "Будь ласка, введіть текст, URL або завантажте файл.";
                    return View("Index");
                }

                result = await _mlService.AnalyzeContentAsync(textToAnalyze, detectedType.ToString());

                if (result != null)
                {
                    ViewBag.Verdict = result.Verdict;
                    ViewBag.Confidence = result.ConfidenceScore;

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _newsCheckService.SaveCheckResultAsync(userId, textToAnalyze, result.Verdict!, result.ConfidenceScore, detectedType);
                        ViewBag.Message = $"Результат успішно збережено в історію! (Формат: {detectedType})";
                    }
                    else
                    {
                        ViewBag.Message = $"Результат не збережено. Увійдіть у систему. (Формат: {detectedType})";
                    }
                }
                else
                {
                    ViewBag.Error = "Помилка: Python-сервер повернув порожню відповідь.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Сталася помилка: {ex.Message}";
            }

            return View("Index");
        }
    }
}