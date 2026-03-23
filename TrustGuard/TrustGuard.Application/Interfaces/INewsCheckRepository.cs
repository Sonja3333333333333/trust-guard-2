using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Interfaces
{
    public interface INewsCheckRepository
    {
        Task AddAsync(NewsCheck newsCheck);
    }
}