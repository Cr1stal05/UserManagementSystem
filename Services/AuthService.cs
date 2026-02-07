using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementSystem.Data;
using UserManagementSystem.Models;
using UserManagementSystem.DTOs;

namespace UserManagementSystem.Services
{
    public interface IAuthService
    {
        Task<string> Register(RegisterRequest request);
        Task<string> Login(LoginRequest request);
        string GenerateJwtToken(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<string> Register(RegisterRequest request)
        {
            // Note: Мы НЕ проверяем уникальность email здесь
            // База данных сама сделает это через уникальный индекс

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RegistrationTime = DateTime.UtcNow,
                LastLoginTime = DateTime.UtcNow,
                Status = UserStatus.Unverified,
                LastActivityTime = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Отправка email асинхронно
                //_ = SendVerificationEmailAsync(user.Email, user.Id);////////////////////////////////

                return "Registration successful. Please check your email for verification.";
            }
            catch (DbUpdateException ex)
            {
                // Important: Обработка нарушения уникальности от БД
                if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    throw new ApplicationException("Email already exists.");
                }
                throw;
            }
        }

        public async Task<string> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ApplicationException("Invalid credentials.");
            }

            // Important: Проверка блокировки
            if (user.Status == UserStatus.Blocked)
            {
                throw new ApplicationException("Account is blocked.");
            }

            // Обновление времени последнего входа
            user.LastLoginTime = DateTime.UtcNow;
            user.LastActivityTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return GenerateJwtToken(user);
        }

        private async Task SendVerificationEmailAsync(string email, Guid userId)
        {
            var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");

            var verificationLink = $"https://yourapp.com/api/auth/verify?token={verificationToken}&userId={userId}";

            await _emailService.SendEmailAsync(
                email,
                "Verify your email",
                $"Please verify your email by clicking: {verificationLink}");
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(
                _configuration["JwtSettings:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("Status", user.Status.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}