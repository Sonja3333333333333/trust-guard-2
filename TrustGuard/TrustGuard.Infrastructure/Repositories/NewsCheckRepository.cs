using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;
using TrustGuard.Infrastructure.Persistence;

namespace TrustGuard.Infrastructure.Repositories
{
    public class NewsCheckRepository : INewsCheckRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public NewsCheckRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(NewsCheck newsCheck)
        {
            await _dbContext.NewsChecks.AddAsync(newsCheck);
            await _dbContext.SaveChangesAsync();
        }
    }
}