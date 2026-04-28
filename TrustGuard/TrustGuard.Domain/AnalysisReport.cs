namespace TrustGuard.Domain.Entities
{
    public class AnalysisReport
    {
        public int Id { get; set; }

        public int NewsCheckId { get; set; }
        public virtual NewsCheck NewsCheck { get; set; } = null!;

        public string? KeyTermsJson { get; set; }
        public int SimilarArticlesCount { get; set; }
        public bool IsUnique { get; set; }
    }
}