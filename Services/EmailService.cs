using MailKit.Net.Smtp;
using MimeKit;

namespace UserManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                "User Management System",
                _configuration["EmailSettings:SenderEmail"]));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;

            email.Body = new TextPart("plain")
            {
                Text = body
            };

            // Note: Асинхронная отправка без ожидания
            _ = Task.Run(async () =>
            {
                try
                {
                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(
                        _configuration["EmailSettings:SmtpServer"],
                        int.Parse(_configuration["EmailSettings:SmtpPort"]),
                        bool.Parse(_configuration["EmailSettings:EnableSsl"]));

                    await smtp.AuthenticateAsync(
                        _configuration["EmailSettings:SenderEmail"],
                        _configuration["EmailSettings:SenderPassword"]);

                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    // Логирование ошибки, но не прерывание потока
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                }
            });
        }
    }
}
