using TrustGuard.Application.Interfaces;
namespace TrustGuard.Web.Models
{
    public class NewsCheckResultViewModel
    {
        public string? Verdict { get; set; }
        public double Confidence { get; set; }
        public string? MlMessage { get; set; }
        public string? Summary { get; set; }
        public List<SourceLink>? OsintLinks { get; set; }
        public List<string>? KeyTriggers { get; set; }
        public int? DomainScore { get; set; }
        public List<string>? DomainFactors { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
