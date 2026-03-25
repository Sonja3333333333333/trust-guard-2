using DocumentFormat.OpenXml.Packaging;
using System.Text;
using TrustGuard.Application.Interfaces;
using UglyToad.PdfPig;

namespace TrustGuard.Infrastructure.Services
{
    public class FileParserService : IFileParserService
    {
        public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("Файл порожній.");

            var extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".txt" => await ParseTxtAsync(fileStream),
                ".pdf" => ParsePdf(fileStream),
                ".docx" => ParseDocx(fileStream),
                _ => throw new ArgumentException($"Формат {extension} не підтримується.")
            };
        }

        private async Task<string> ParseTxtAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private string ParsePdf(Stream stream)
        {
            var text = new StringBuilder();
            using (var pdf = PdfDocument.Open(stream))
            {
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }
            return text.ToString();
        }

        private string ParseDocx(Stream stream)
        {
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            return wordDoc.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
        }
    }
}