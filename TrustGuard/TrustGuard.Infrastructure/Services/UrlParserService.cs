using SmartReader;
using System.Text.RegularExpressions;
using TrustGuard.Application.Interfaces;
using Microsoft.Extensions.Logging; // Обов'язково додаємо для логера

namespace TrustGuard.Infrastructure.Services
{
    public class UrlParserService : IUrlParserService
    {
        private readonly ILogger<UrlParserService> _logger; // Створюємо змінну

        // Конструктор: .NET сам підкине сюди логер
        public UrlParserService(ILogger<UrlParserService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractTextFromUrlAsync(string url)
        {
            try
            {
                _logger.LogInformation("Починаю парсинг URL: {Url}", url);

                var article = await Reader.ParseArticleAsync(url);

                if (!article.IsReadable)
                {
                    _logger.LogWarning("SmartReader не зміг знайти статтю. Можливо, це головна сторінка сайту, а не конкретна новина.");
                    return "Не вдалося розпізнати головну статтю на цій сторінці.";
                }

                string cleanText = article.TextContent;
                cleanText = Regex.Replace(cleanText, @"\n{3,}", "\n\n").Trim();

                // МАГІЯ: ВИВОДИМО ТЕКСТ У КОНСОЛЬ
                _logger.LogInformation("\n=== ВИТЯГНУТИЙ ТЕКСТ З URL ===\n{Text}\n==============================", cleanText);

                return cleanText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при обробці URL!");
                throw new Exception($"Помилка при обробці URL: {ex.Message}");
            }
        }
    }
}