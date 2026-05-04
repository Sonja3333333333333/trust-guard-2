using System;
using System.Collections.Generic;
using System.Linq; // Необхідно для роботи методу Any()
using System.Text.Json;
using System.Threading.Tasks;
using TrustGuard.Application.Interfaces;
using TrustGuard.Domain;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Services
{
    public class DomainScoringService : IDomainScoringService
    {
        private readonly IDomainTrustRepository _cacheRepository;
        private readonly IDomainRepository _whitelistRepository;

        // Впроваджуємо обидва репозиторії
        public DomainScoringService(
            IDomainTrustRepository cacheRepository,
            IDomainRepository whitelistRepository)
        {
            _cacheRepository = cacheRepository;
            _whitelistRepository = whitelistRepository;
        }

        public async Task<DomainTrustRecord> ScoreDomainAsync(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return CreateErrorRecord("Невалідний формат URL");
            }

            string domainName = uri.Host.ToLower().Replace("www.", "");

            var existingRecord = await _cacheRepository.GetByDomainAsync(domainName);
            if (existingRecord != null)
            {
                return existingRecord;
            }

            int score = 40;
            var factors = new List<string>();

            var trustedDomains = await _whitelistRepository.GetTrustedDomainUrlsAsync();
            if (trustedDomains.Any(td => domainName.Contains(td)))
            {
                score += 50; 
                factors.Add("Джерело входить до нашого списку перевірених медіа");
            }

            if (uri.Scheme == "https")
            {
                score += 10;
                factors.Add("Захищене з'єднання (HTTPS)");
            }
            else
            {
                score -= 20;
                factors.Add("Небезпечне з'єднання (HTTP). Дані не зашифровані.");
            }
            if (domainName.EndsWith(".gov") || domainName.EndsWith(".edu"))
            {
                score += 30;
                factors.Add("Офіційний урядовий або освітній домен (висока довіра)");
            }
            else if (domainName.EndsWith(".xyz") || domainName.EndsWith(".tk") || domainName.EndsWith(".biz"))
            {
                score -= 30;
                factors.Add("Використовується підозріла доменна зона, типова для спаму");
            }
            else
            {
                factors.Add("Стандартна доменна зона");
            }

            score = Math.Clamp(score, 0, 100);

            var newRecord = new DomainTrustRecord
            {
                DomainName = domainName,
                TrustScore = score,
                FactorsJson = JsonSerializer.Serialize(factors),
                AnalyzedAt = DateTime.UtcNow
            };

            await _cacheRepository.SaveRecordAsync(newRecord);

            return newRecord;
        }

        private DomainTrustRecord CreateErrorRecord(string errorMessage)
        {
            return new DomainTrustRecord
            {
                DomainName = "unknown",
                TrustScore = 0,
                FactorsJson = JsonSerializer.Serialize(new List<string> { $"Помилка: {errorMessage}" }),
                AnalyzedAt = DateTime.UtcNow
            };
        }
    }
}