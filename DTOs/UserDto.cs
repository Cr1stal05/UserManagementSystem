using System;
using UserManagementSystem.Models;

namespace UserManagementSystem.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime RegistrationTime { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastActivityTime { get; set; }
    }
}