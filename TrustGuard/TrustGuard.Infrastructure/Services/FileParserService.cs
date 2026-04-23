using DocumentFormat.OpenXml.Packaging;
using System.Text;
using TrustGuard.Application.Interfaces;
using UglyToad.PdfPig;
using Tesseract; // Додали Tesseract
using Microsoft.Extensions.Logging; 


namespace TrustGuard.Infrastructure.Services
{
    public class FileParserService : IFileParserService
    {
        private readonly ILogger<FileParserService> _logger;

        // Конструктор: .NET сам передасть сюди логер (Dependency Injection)
        public FileParserService(ILogger<FileParserService> logger)
        {
            _logger = logger;
        }
        public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("Файл порожній.");

            var extension = Path.GetExtension(fileName).ToLower();
            _logger.LogInformation("Починаю парсинг файлу: {FileName} (формат: {Extension})", fileName, extension);
            return extension switch
            {
                ".txt" => await ParseTxtAsync(fileStream),
                ".pdf" => ParsePdf(fileStream),
                ".docx" => ParseDocx(fileStream),
                // Додаємо підтримку картинок
                ".jpg" or ".jpeg" or ".png" => ParseImage(fileStream),
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

        // --- НОВИЙ МЕТОД ДЛЯ ФОТО ---
        private string ParseImage(Stream stream)
        {
            try
            {
                // Конвертуємо Stream у byte[], бо Tesseract вимагає цього
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                byte[] imageBytes = ms.ToArray();

                // Шлях до папки зі словниками (tessdata). 
                // "ukr+eng" означає, що ми розпізнаємо і українську, і англійську одночасно
                using var engine = new TesseractEngine(@"./tessdata", "ukr+eng", EngineMode.Default);
                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);
                var extractedText = page.GetText();
                _logger.LogInformation("\n=== ВИТЯГНУТИЙ ТЕКСТ З ФОТО ===\n{Text}\n===============================", extractedText);
                return extractedText;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка при розпізнаванні тексту з фото: {ex.Message}");
            }
        }
    }
}