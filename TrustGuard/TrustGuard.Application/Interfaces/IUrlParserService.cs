namespace TrustGuard.Application.Interfaces
{
    public interface IUrlParserService
    {
        Task<string> ExtractTextFromUrlAsync(string url);
    }
}