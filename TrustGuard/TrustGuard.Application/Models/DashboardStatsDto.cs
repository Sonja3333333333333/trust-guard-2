using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Models
{
    public class DashboardStatsDto
    {
        public int TotalChecks { get; set; }
        public double FakePercentage { get; set; }
        public double SafetyScore { get; set; }
        public List<NewsCheck> RecentHistory { get; set; } = new();
    }
}