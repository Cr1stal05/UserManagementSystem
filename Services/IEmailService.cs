namespace UserManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        // IMPORTANT
        Task SendConfirmationEmailAsync(string email, string token);
    }
}
