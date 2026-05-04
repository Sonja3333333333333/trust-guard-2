using System;
using System.ComponentModel.DataAnnotations;

namespace TrustGuard.Domain
{
    public class DomainTrustRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string DomainName { get; set; } = string.Empty;

        public int TrustScore { get; set; }

        public string FactorsJson { get; set; } = "[]";

        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }
}