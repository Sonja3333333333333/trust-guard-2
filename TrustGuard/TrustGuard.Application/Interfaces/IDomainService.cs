using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrustGuard.Application.Interfaces
{
    public interface IDomainService
    {
        Task<List<string>> GetTrustedDomainsAsync();
    }
}