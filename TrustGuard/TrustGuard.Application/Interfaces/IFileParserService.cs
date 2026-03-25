using System.IO;

namespace TrustGuard.Application.Interfaces
{
    public interface IFileParserService
    {
        Task<string> ExtractTextAsync(Stream fileStream, string fileName);
    }
}