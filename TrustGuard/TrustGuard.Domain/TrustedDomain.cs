using System.ComponentModel.DataAnnotations;

namespace TrustGuard.Domain.Entities
{
    public class TrustedDomain
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string DomainUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string SourceName { get; set; } = string.Empty;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}