using TrustGuard.Application.Interfaces;
using TrustGuard.Domain.Entities;

namespace TrustGuard.Infrastructure.Services
{
    public class NewsCheckService : INewsCheckService
    {
        private readonly INewsCheckRepository _repository; 

        public NewsCheckService(INewsCheckRepository repository)
        {
            _repository = repository;
        }

        public async Task SaveCheckResultAsync(string userId, string content, string verdictString, float confidence)
        {
            if (!Enum.TryParse<Verdict>(verdictString, true, out var finalVerdict))
            {
                finalVerdict = Verdict.Uncertain;
            }

            var newsCheck = new NewsCheck
            {
                UserId = userId,
                RawContent = content,
                CheckDate = DateTime.UtcNow,
                ConfidenceScore = confidence,
                ContentType = ContentType.Text, 
                Verdict = finalVerdict          
            };

            await _repository.AddAsync(newsCheck);
        }
    }
}
