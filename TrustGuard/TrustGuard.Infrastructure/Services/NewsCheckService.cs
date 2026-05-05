using TrustGuard.Application.Interfaces;
using TrustGuard.Application.Models;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Infrastructure.Services
{
    public class NewsCheckService : INewsCheckService
    {
        private readonly INewsCheckRepository _repository; 

        public NewsCheckService(INewsCheckRepository repository)
        {
            _repository = repository;
        }

        public async Task SaveCheckResultAsync(
             string userId,
             string content,
             string verdictString,
             float confidence,
             ContentType contentType,
             List<SourceLink>? osintLinks = null,
             string? summary = null,
             List<string>? keyTriggers = null)
        {
            if (!Enum.TryParse<Verdict>(verdictString, true, out var finalVerdict))
            {
                finalVerdict = Verdict.Uncertain;
            }

            var newsCheck = new NewsCheck
            {
                UserId = userId,
                RawContent = content,
                CheckDate = DateTime.UtcNow,
                ConfidenceScore = confidence,
                ContentType = contentType, 
                Verdict = finalVerdict,
                Summary = summary,
                ExternalSources = new List<ExternalSource>(),
                KeyTriggers = new List<KeyTrigger>()
            };

            if (osintLinks != null && osintLinks.Any())
            {
                foreach (var link in osintLinks)
                {
                    newsCheck.ExternalSources.Add(new ExternalSource
                    {
                        Title = link.Name ?? "Невідоме джерело",
                        Url = link.Url ?? "",
                        SourceName = "Google Search"
                    });
                }
            }

            if (keyTriggers != null && keyTriggers.Any())
            {
                foreach (var word in keyTriggers)
                {
                    newsCheck.KeyTriggers.Add(new KeyTrigger
                    {
                        Word = word
                    });
                }
            }

            await _repository.AddAsync(newsCheck);
        }
        public async Task<List<NewsCheck>> GetRecentUserHistoryAsync(string userId)
        {
            var history = await _repository.GetUserHistoryAsync(userId);

            return history.Take(10).ToList();
        }

        public async Task<List<NewsCheck>> GetFullUserHistoryAsync(string userId)
        {
            return await _repository.GetUserHistoryAsync(userId);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string userId)
        {
            var stats = new DashboardStatsDto();

            var fullHistory = await _repository.GetUserHistoryAsync(userId);

            stats.TotalChecks = fullHistory.Count;

            if (stats.TotalChecks > 0)
            {
                var fakeCount = fullHistory.Count(c => c.Verdict == Verdict.Fake);

                stats.FakePercentage = Math.Round((double)fakeCount / stats.TotalChecks * 100, 1);
                stats.SafetyScore = 100 - stats.FakePercentage;
            }
            else
            {
                stats.FakePercentage = 0;
                stats.SafetyScore = 100;
            }

            stats.RecentHistory = fullHistory.Take(10).ToList();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .Reverse()
                .ToList();

            foreach (var date in last7Days)
            {
                stats.ChartLabels.Add(date.ToString("dd.MM"));

                var checksOnDate = fullHistory.Where(c => c.CheckDate.Date == date).ToList();

                stats.ChartRealData.Add(checksOnDate.Count(c => c.Verdict == TrustGuard.Domain.Entities.Verdict.Real));
                stats.ChartFakeData.Add(checksOnDate.Count(c => c.Verdict == TrustGuard.Domain.Entities.Verdict.Fake));
            }

            return stats;
        }

        public async Task<NewsCheck?> GetCheckDetailsAsync(int id, string userId)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check != null && check.UserId == userId)
            {
                return check;
            }
            return null;
        }
    }
}
