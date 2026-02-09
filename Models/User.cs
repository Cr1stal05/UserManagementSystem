using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementSystem.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime LastLoginTime { get; set; }

        public DateTime RegistrationTime { get; set; }

        [Required]
        public UserStatus Status { get; set; }

        public DateTime? LastActivityTime { get; set; }

        public string? EmailConfirmationToken { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;
    }

    public enum UserStatus
    {
        Unverified = 0,
        Active = 1,
        Blocked = 2
    }
}
