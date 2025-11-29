using System.Net;
using System.Net.Mail;

namespace Firmeza.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachmentBytes, string attachmentName);
    }

    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _email;
        private readonly string _password;
        private readonly bool _enableSsl;

        public EmailService()
        {
            _host = Environment.GetEnvironmentVariable("SMTP_HOST")!;
            _port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!);
            _email = Environment.GetEnvironmentVariable("SMTP_EMAIL")!;
            _password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!;
            _enableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL")!);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_email, "Firmeza");
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_email, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(message);
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachmentBytes, string attachmentName)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_email, "Firmeza");
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            // Add PDF attachment
            using var stream = new MemoryStream(attachmentBytes);
            var attachment = new Attachment(stream, attachmentName, "application/pdf");
            message.Attachments.Add(attachment);

            using var smtp = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_email, _password),
                EnableSsl = _enableSsl
            };

            await smtp.SendMailAsync(message);
        }
    }
}