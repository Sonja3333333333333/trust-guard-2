namespace TrustGuard.Domain.Entities
{
    public class ExternalSource
    {
        public int Id { get; set; }

        public int NewsCheckId { get; set; }
        public virtual NewsCheck NewsCheck { get; set; } = null!;

        public required string Title { get; set; }
        public required string Url { get; set; }
        public string? SourceName { get; set; }
    }
}