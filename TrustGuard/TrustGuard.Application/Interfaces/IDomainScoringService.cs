using System.Threading.Tasks;
using TrustGuard.Domain;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Application.Interfaces
{
    public interface IDomainScoringService
    {
        Task<DomainTrustRecord> ScoreDomainAsync(string url);
    }
}