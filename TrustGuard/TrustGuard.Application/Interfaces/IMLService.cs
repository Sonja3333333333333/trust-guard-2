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
        Task<MlAnalysisResponse?> AnalyzeContentAsync(string text, string contentType = "Text");

    }
}