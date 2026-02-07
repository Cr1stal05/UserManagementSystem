using System.ComponentModel.DataAnnotations;

namespace UserManagementSystem.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(1)] // Пароль может быть от 1 символа
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
