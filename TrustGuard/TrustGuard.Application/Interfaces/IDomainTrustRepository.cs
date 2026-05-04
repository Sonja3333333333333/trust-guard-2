using TrustGuard.Domain;

namespace TrustGuard.Application.Interfaces
{
    public interface IDomainTrustRepository
    {
        Task<DomainTrustRecord?> GetByDomainAsync(string domainName);

        Task SaveRecordAsync(DomainTrustRecord record);
    }
}