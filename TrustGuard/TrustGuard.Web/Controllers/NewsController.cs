using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;
using TrustGuard.Web.Extensions;
using TrustGuard.Web.Models;

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
            var model = TempData.Get<NewsCheckResultViewModel>("AnalysisResult") ?? new NewsCheckResultViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(string? textContent, string? urlContent, IFormFile? documentFile, IFormFile? imageFile)
        {
            string textToAnalyze = "";
            ContentType detectedType = ContentType.Text;
            MlAnalysisResponse? result = null;

            var viewModel = new NewsCheckResultViewModel();

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
                        viewModel.Error = "Не вдалося витягнути текст за цим посиланням. Можливо, сайт блокує парсинг або там немає статті.";
                        TempData.Put("AnalysisResult", viewModel);
                        return RedirectToAction(nameof(Index));
                    }

                    var domainRecord = await _domainScoringService.ScoreDomainAsync(urlContent);
                    viewModel.DomainScore = domainRecord.TrustScore;
                    if (!string.IsNullOrEmpty(domainRecord.FactorsJson))
                    {
                        viewModel.DomainFactors = JsonSerializer.Deserialize<List<string>>(domainRecord.FactorsJson);
                    }
                }
                else if (documentFile != null && documentFile.Length > 0)
                {
                    detectedType = ContentType.Document;
                    using var stream = documentFile.OpenReadStream();
                    textToAnalyze = await _fileParserService.ExtractTextAsync(stream, documentFile.FileName);

                    if (string.IsNullOrWhiteSpace(textToAnalyze))
                    {
                        viewModel.Error = "Не вдалося витягнути текст із файлу. Можливо, він порожній або це відскановані картинки.";
                        TempData.Put("AnalysisResult", viewModel);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else if (imageFile != null && imageFile.Length > 0)
                {
                    detectedType = ContentType.Document;
                    using var stream = imageFile.OpenReadStream();
                    textToAnalyze = await _fileParserService.ExtractTextAsync(stream, imageFile.FileName);

                    if (string.IsNullOrWhiteSpace(textToAnalyze))
                    {
                        viewModel.Error = "Не вдалося розпізнати текст на зображенні. Можливо, текст занадто розмитий.";
                        TempData.Put("AnalysisResult", viewModel);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    viewModel.Error = "Будь ласка, введіть текст, URL або завантажте файл.";
                    TempData.Put("AnalysisResult", viewModel);
                    return RedirectToAction(nameof(Index));
                }

                result = await _mlService.AnalyzeContentAsync(textToAnalyze, detectedType.ToString());

                if (result != null && result.MlAnalysis != null)
                {
                    viewModel.Verdict = result.MlAnalysis.Verdict;
                    viewModel.Confidence = result.MlAnalysis.ConfidenceScore;
                    viewModel.MlMessage = result.MlAnalysis.Message;
                    viewModel.Summary = result.MlAnalysis.Summary;
                    viewModel.OsintLinks = result.OsintAnalysis?.Links;
                    viewModel.KeyTriggers = result.MlAnalysis.KeyTriggers;

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _newsCheckService.SaveCheckResultAsync(
                            userId,
                            textToAnalyze,
                            result.MlAnalysis.Verdict!,
                            result.MlAnalysis.ConfidenceScore,
                            detectedType,
                            result.OsintAnalysis?.Links,
                            result.MlAnalysis.Summary,
                            result.MlAnalysis.KeyTriggers);

                        viewModel.Message = $"Результат успішно збережено в історію! (Формат: {detectedType})";
                    }
                    else
                    {
                        viewModel.Message = $"Результат не збережено. Увійдіть у систему. (Формат: {detectedType})";
                    }
                }
                else
                {
                    viewModel.Error = "Помилка: Python-сервер повернув порожню відповідь.";
                }
            }
            catch (Exception ex)
            {
                viewModel.Error = $"Сталася помилка: {ex.Message}";
            }

            TempData.Put("AnalysisResult", viewModel);
            
            return RedirectToAction(nameof(Index));
        }
    }
}