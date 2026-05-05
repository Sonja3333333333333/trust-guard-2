using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;
using TrustGuard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<NewsCheck>> GetUserHistoryAsync(string userId)
        {
            return await _dbContext.NewsChecks
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CheckDate) 
                .ToListAsync();
        }

        public async Task<NewsCheck?> GetByIdAsync(int id)
        {
            return await _dbContext.NewsChecks
                .Include(n => n.ExternalSources)
                .Include(n => n.KeyTriggers)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}