using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Web.Controllers
{
    public class NewsController : Controller
    {
        private readonly IMlService _mlService;
        private readonly INewsCheckService _newsCheckService;
        private readonly IFileParserService _fileParserService;
        private readonly IUrlParserService _urlParserService;
        private readonly IDomainScoringService _domainScoringService;

        public NewsController(
            IMlService mlService,
            INewsCheckService newsCheckService,
            IFileParserService fileParserService,
            IUrlParserService urlParserService,
            IDomainScoringService domainScoringService)
        {
            _mlService = mlService;
            _newsCheckService = newsCheckService;
            _fileParserService = fileParserService;
            _urlParserService = urlParserService;
            _domainScoringService = domainScoringService;
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
                else if (!string.IsNullOrWhiteSpace(urlContent))
                {
                    detectedType = ContentType.Url;
                    textToAnalyze = await _urlParserService.ExtractTextFromUrlAsync(urlContent);

                    if (string.IsNullOrWhiteSpace(textToAnalyze) || textToAnalyze.StartsWith("Не вдалося"))
                    {
                        ViewBag.Error = "Не вдалося витягнути текст за цим посиланням. Можливо, сайт блокує парсинг або там немає статті.";
                        return View("Index");
                    }

                    var domainRecord = await _domainScoringService.ScoreDomainAsync(urlContent);
                    ViewBag.DomainScore = domainRecord.TrustScore;
                    if (!string.IsNullOrEmpty(domainRecord.FactorsJson))
                    {
                        ViewBag.DomainFactors = JsonSerializer.Deserialize<List<string>>(domainRecord.FactorsJson);
                    }
                }
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
                    detectedType = ContentType.Document;

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

                if (result != null && result.MlAnalysis != null)
                {
                    ViewBag.Verdict = result.MlAnalysis.Verdict;
                    ViewBag.Confidence = result.MlAnalysis.ConfidenceScore;
                    ViewBag.MlMessage = result.MlAnalysis.Message;

                    ViewBag.Summary = result.MlAnalysis.Summary;

                    ViewBag.OsintLinks = result.OsintAnalysis?.Links;

                    ViewBag.KeyTriggers = result.MlAnalysis.KeyTriggers;

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _newsCheckService.SaveCheckResultAsync(
                            userId,
                            textToAnalyze,
                            result.MlAnalysis.Verdict!,
                            result.MlAnalysis.ConfidenceScore,
                            detectedType,
                            result.OsintAnalysis?.Links);

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