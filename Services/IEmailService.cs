namespace UserManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        Task SendConfirmationEmailAsync(string email, string token);
    }
}
