using System.Text.Json.Serialization;

namespace TrustGuard.Application.Interfaces
{
    public class MlAnalysisRequest
    {
        [JsonPropertyName("content")]
        public required string Content { get; set; }

        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }
        public List<string> TrustedDomains { get; set; } = new List<string>();
    }

    public class MlAnalysisResponse
    {
        [JsonPropertyName("mlAnalysis")]
        public MlAnalysisData? MlAnalysis { get; set; }

        [JsonPropertyName("osintAnalysis")]
        public OsintAnalysisData? OsintAnalysis { get; set; }
    }

    // Дані ШІ
    public class MlAnalysisData
    {
        [JsonPropertyName("verdict")]
        public string? Verdict { get; set; }

        [JsonPropertyName("confidenceScore")]
        public float ConfidenceScore { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    // Дані Google Search (OSINT)
    public class OsintAnalysisData
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("trustedSourcesFound")]
        public int TrustedSourcesFound { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("links")]
        public List<SourceLink>? Links { get; set; }
    }

    public class SourceLink
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public interface IMlService
    {
        Task<MlAnalysisResponse?> AnalyzeContentAsync(string text, string contentType = "Text");
    }
}