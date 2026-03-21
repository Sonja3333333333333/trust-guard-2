using System.Net.Http.Json;
using TrustGuard.Application.Interfaces; 

namespace TrustGuard.Infrastructure.Services
{
    public class FastApiMlService : IMlService
    {
        private readonly HttpClient _httpClient;

        public FastApiMlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/");
        }

        public async Task<MlAnalysisResponse?> AnalyzeContentAsync(string text, string contentType = "Text")
        {
            var requestData = new MlAnalysisRequest
            {
                Content = text,
                ContentType = contentType
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