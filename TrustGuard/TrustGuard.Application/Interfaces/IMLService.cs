using System.Text.Json.Serialization;

namespace TrustGuard.Application.Interfaces
{
    public class MlAnalysisRequest
    {
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }
    }

    public class MlAnalysisResponse
    {
        [JsonPropertyName("verdict")]
        public string? Verdict { get; set; }

        [JsonPropertyName("confidenceScore")]
        public float ConfidenceScore { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public interface IMlService
    {
        // Старий метод: для тексту та URL (відправляється як JSON)
        Task<MlAnalysisResponse?> AnalyzeContentAsync(string text, string contentType = "Text");

        // НОВИЙ МЕТОД: для PDF, Word, Зображень (відправляється як файл)
        Task<MlAnalysisResponse?> AnalyzeFileAsync(Stream fileStream, string fileName, string contentType);
    }
}