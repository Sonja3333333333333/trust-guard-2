namespace TrustGuard.Domain.Entities
{
    public class NewsCheck
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;

        public ContentType ContentType { get; set; }
        public required string RawContent { get; set; }
        public string? TitleExtracted { get; set; }

        public Verdict Verdict { get; set; }
        public float ConfidenceScore { get; set; }
        public DateTime CheckDate { get; set; } = DateTime.UtcNow;

        // Навігаційні властивості (Зв'язки)
        public virtual AnalysisReport? AnalysisReport { get; set; }
        public virtual ICollection<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();

        // Змінили на колекцію, як ти і пропонувала! (1:N)
        public virtual ICollection<MediaMetadata> MediaMetadatas { get; set; } = new List<MediaMetadata>();

        public string? Summary { get; set; }

        public virtual ICollection<KeyTrigger> KeyTriggers { get; set; } = new List<KeyTrigger>();
    }

}
