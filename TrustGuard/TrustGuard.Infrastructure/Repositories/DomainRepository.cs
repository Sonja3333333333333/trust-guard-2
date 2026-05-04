using Microsoft.EntityFrameworkCore;
using TrustGuard.Application.Interfaces;
using TrustGuard.Infrastructure.Persistence;

namespace TrustGuard.Infrastructure.Repositories
{

    public class DomainRepository : IDomainRepository
    {
        private readonly ApplicationDbContext _context;

        public DomainRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetTrustedDomainUrlsAsync()
        {
            return await _context.TrustedDomains
                .AsNoTracking()
                .Select(d => d.DomainUrl)
                .ToListAsync();
        }
    }
}