using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Interfaces
{
    public interface INewsCheckRepository
    {
        Task AddAsync(NewsCheck newsCheck);

        Task<List<NewsCheck>> GetUserHistoryAsync(string userId);

        Task<NewsCheck?> GetByIdAsync(int id);
    }
}