using TrustGuard.Application.Interfaces;

namespace TrustGuard.Infrastructure.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;

        public DomainService(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task<List<string>> GetTrustedDomainsAsync()
        {
            return await _domainRepository.GetTrustedDomainUrlsAsync();
        }
    }
}