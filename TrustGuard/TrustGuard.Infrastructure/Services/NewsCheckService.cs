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

        public async Task SaveCheckResultAsync(string userId, string content, string verdictString, float confidence, ContentType contentType)
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
                Verdict = finalVerdict          
            };

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
