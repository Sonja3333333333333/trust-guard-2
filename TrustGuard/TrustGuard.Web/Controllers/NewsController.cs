using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustGuard.Application.Interfaces;
using Microsoft.AspNetCore.Http; // Додали для IFormFile
using System.IO; // Додали для роботи з файлами

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

        // Зверни увагу: тепер ми приймаємо всі 4 параметри з нашої форми
        [HttpPost]
        public async Task<IActionResult> Analyze(string? textContent, string? urlContent, IFormFile? documentFile, IFormFile? imageFile)
        {
            string finalDataToSave = ""; // Те, що ми запишемо в базу (текст, URL або назву файлу)
            string detectedType = "";
            MlAnalysisResponse? result = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(textContent))
                {
                    detectedType = "Text";
                    finalDataToSave = textContent;
                    result = await _mlService.AnalyzeContentAsync(textContent, detectedType);
                }
                else if (!string.IsNullOrWhiteSpace(urlContent))
                {
                    detectedType = "URL";
                    finalDataToSave = urlContent;
                    result = await _mlService.AnalyzeContentAsync(urlContent, detectedType);
                }
                else if (documentFile != null && documentFile.Length > 0)
                {
                    detectedType = "Document";
                    finalDataToSave = documentFile.FileName;

                    if (Path.GetExtension(documentFile.FileName).ToLower() == ".txt")
                    {
                        using var reader = new StreamReader(documentFile.OpenReadStream());
                        var txtContent = await reader.ReadToEndAsync();
                        result = await _mlService.AnalyzeContentAsync(txtContent, detectedType);
                    }
                    else
                    {
                        // PDF та DOCX відправляємо як файли!
                        using var stream = documentFile.OpenReadStream();
                        result = await _mlService.AnalyzeFileAsync(stream, documentFile.FileName, detectedType);
                    }
                }
                else if (imageFile != null && imageFile.Length > 0)
                {
                    detectedType = "Image";
                    finalDataToSave = imageFile.FileName;

                    // Зображення відправляємо як файл!
                    using var stream = imageFile.OpenReadStream();
                    result = await _mlService.AnalyzeFileAsync(stream, imageFile.FileName, detectedType);
                }
                else
                {
                    ViewBag.Error = "Будь ласка, введіть текст, URL або завантажте файл.";
                    return View("Index");
                }

                // Обробляємо успішну відповідь від Python
                if (result != null)
                {
                    ViewBag.Verdict = result.Verdict;
                    ViewBag.Confidence = result.ConfidenceScore;

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Якщо це файл, в базу запишеться його назва. Якщо текст - сам текст.
                        await _newsCheckService.SaveCheckResultAsync(userId, finalDataToSave, result.Verdict!, result.ConfidenceScore);
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
                ViewBag.Error = $"Помилка з'єднання: {ex.Message}. Переконайтеся, що FastAPI запущений.";
            }

            return View("Index");
        }
    }
}