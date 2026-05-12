using System.Net.Http.Json;
using TrustGuard.Application.Interfaces;
using Microsoft.Extensions.Configuration; 

namespace TrustGuard.Infrastructure.Services
{
    public class FastApiMlService : IMlService
    {
        private readonly HttpClient _httpClient;
        private readonly IDomainService _domainService;

        public FastApiMlService(HttpClient httpClient, IDomainService domainService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var mlApiUrl = configuration["MLApiUrl"] ?? "http://127.0.0.1:8000/";
            _httpClient.BaseAddress = new Uri(mlApiUrl);
            _domainService = domainService;
        }

        public async Task<MlAnalysisResponse?> AnalyzeContentAsync(string text, string contentType = "Text")
        {
            var trustedDomainsList = await _domainService.GetTrustedDomainsAsync();

            var requestData = new MlAnalysisRequest
            {
                Content = text,
                ContentType = contentType,
                TrustedDomains = trustedDomainsList
            };

            var response = await _httpClient.PostAsJsonAsync("api/analyze", requestData);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MlAnalysisResponse>();
            }

            return null;
        }

    }
}