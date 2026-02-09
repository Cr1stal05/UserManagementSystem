using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementSystem.Models;
using UserManagementSystem.Services;
using UserManagementSystem.DTOs;
using UserManagementSystem.Data;

namespace UserManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.Register(request);
                return Ok(new { message = result });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            await _authService.ConfirmEmailAsync(token);
            return Ok("Email confirmed successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.Login(request);
                return Ok(new { token });
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.Status == UserStatus.Unverified)
            {
                user.Status = UserStatus.Active;
                await _context.SaveChangesAsync();
            }

            return Redirect("/login?verified=true");
        }
    }
}
