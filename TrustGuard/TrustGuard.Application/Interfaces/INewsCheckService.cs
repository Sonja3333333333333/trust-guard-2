using TrustGuard.Application.Models;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Interfaces
{
    public interface INewsCheckService
    {
        Task SaveCheckResultAsync(string userId, string content, string verdict, float confidence);
        Task<List<NewsCheck>> GetRecentUserHistoryAsync(string userId);
        Task<List<NewsCheck>> GetFullUserHistoryAsync(string userId);
        Task<DashboardStatsDto> GetDashboardStatsAsync(string userId);
        Task<NewsCheck?> GetCheckDetailsAsync(int id, string userId);
    }
}