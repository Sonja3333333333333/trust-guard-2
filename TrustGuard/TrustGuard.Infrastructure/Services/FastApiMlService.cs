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

        // НОВИЙ МЕТОД ДЛЯ ФАЙЛІВ
        public async Task<MlAnalysisResponse?> AnalyzeFileAsync(Stream fileStream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();

            // 1. Запаковуємо сам файл
            var fileContent = new StreamContent(fileStream);
            content.Add(fileContent, "file", fileName); // "file" - це ім'я параметра, яке буде чекати Python

            // 2. Додаємо тип контенту (Document або Image)
            content.Add(new StringContent(contentType), "content_type");

            // 3. Відправляємо на спеціальний маршрут для файлів
            var response = await _httpClient.PostAsync("api/analyze/file", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MlAnalysisResponse>();
            }

            return null;
        }
    }
}