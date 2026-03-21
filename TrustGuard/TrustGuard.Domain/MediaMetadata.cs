namespace TrustGuard.Domain.Entities
{
    public class MediaMetadata
    {
        public int Id { get; set; }

        public int NewsCheckId { get; set; }
        public virtual NewsCheck NewsCheck { get; set; } = null!;

        public required string ImagePath { get; set; }
        public float ManipulationScore { get; set; }
        public string? SourceOriginUrl { get; set; }
    }
}