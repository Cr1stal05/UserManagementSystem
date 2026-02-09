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

        // IMPORTANT: универсальный метод отправки email
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                "User Management System",
                _configuration["EmailSettings:SenderEmail"]));

            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart("plain")
            {
                Text = body
            };

            try
            {
                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    bool.Parse(_configuration["EmailSettings:EnableSsl"])
                );

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:SenderPassword"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // IMPORTANT: email errors must not break registration
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        // IMPORTANT: email verification logic
        public async Task SendConfirmationEmailAsync(string email, string token)
        {
            var baseUrl = _configuration["App:BaseUrl"];

            var confirmationLink =
                $"{baseUrl}/api/auth/confirm-email?token={token}";

            var body =
                $"Please confirm your email by clicking the link:\n{confirmationLink}";

            await SendEmailAsync(
                email,
                "Confirm your email",
                body
            );
        }
    }
}
