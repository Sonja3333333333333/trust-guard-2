using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration; // Підключаємо конфігурацію
using TrustGuard.Application.Interfaces;

namespace TrustGuard.Infrastructure.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _config;

        // Інжектимо напряму IConfiguration
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Витягуємо дані прямо з appsettings.json на місці
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderPassword = _config["EmailSettings:SenderPassword"];
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = int.Parse(_config["EmailSettings:Port"]!);

            using var client = new SmtpClient(smtpServer, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!, "TrustGuard Support"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}