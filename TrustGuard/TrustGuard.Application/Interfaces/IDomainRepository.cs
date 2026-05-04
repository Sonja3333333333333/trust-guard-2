namespace TrustGuard.Application.Interfaces
{
    public interface IDomainRepository
    {
        Task<List<string>> GetTrustedDomainUrlsAsync();
    }
}