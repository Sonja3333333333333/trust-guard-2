using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Interfaces
{
    public interface INewsCheckService
    {
        Task SaveCheckResultAsync(string userId, string content, string verdict, float confidence);
    }
}