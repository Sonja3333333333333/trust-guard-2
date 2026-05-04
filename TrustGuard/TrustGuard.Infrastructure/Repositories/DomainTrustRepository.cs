using Microsoft.EntityFrameworkCore;
using TrustGuard.Application.Interfaces;
using TrustGuard.Domain;
using TrustGuard.Infrastructure.Persistence;

namespace TrustGuard.Infrastructure.Repositories
{
    public class DomainTrustRepository : IDomainTrustRepository
    {
        private readonly ApplicationDbContext _context;

        public DomainTrustRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DomainTrustRecord?> GetByDomainAsync(string domainName)
        {
            return await _context.DomainTrustRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DomainName == domainName);
        }

        public async Task SaveRecordAsync(DomainTrustRecord record)
        {
            if (record.Id == 0)
            {
                await _context.DomainTrustRecords.AddAsync(record);
            }
            else
            {
                _context.DomainTrustRecords.Update(record);
            }

            await _context.SaveChangesAsync();
        }
    }
}