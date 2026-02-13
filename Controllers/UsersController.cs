using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagementSystem.Data;
using UserManagementSystem.Models;
using UserManagementSystem.DTOs;

namespace UserManagementSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "lastLoginTime",
            [FromQuery] string sortOrder = "desc")
        {
            IQueryable<User> query = _context.Users;

            query = sortBy.ToLower() switch
            {
                "name" => sortOrder == "asc" ?
                    query.OrderBy(u => u.Name) :
                    query.OrderByDescending(u => u.Name),
                "email" => sortOrder == "asc" ?
                    query.OrderBy(u => u.Email) :
                    query.OrderByDescending(u => u.Email),
                "registrationtime" => sortOrder == "asc" ?
                    query.OrderBy(u => u.RegistrationTime) :
                    query.OrderByDescending(u => u.RegistrationTime),
                _ => sortOrder == "asc" ?
                    query.OrderBy(u => u.LastLoginTime) :
                    query.OrderByDescending(u => u.LastLoginTime)
            };

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    LastLoginTime = u.LastLoginTime,
                    RegistrationTime = u.RegistrationTime,
                    Status = u.Status,
                    LastActivityTime = u.LastActivityTime
                })
                .ToListAsync();

            return Ok(new
            {
                users,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpPost("bulk-action")]
        public async Task<IActionResult> BulkAction([FromBody] BulkActionRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var users = await _context.Users
                .Where(u => request.UserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                switch (request.Action)
                {
                    case "block":
                        user.Status = UserStatus.Blocked;
                        break;
                    case "unblock":
                        user.Status = UserStatus.Active;
                        break;
                    case "delete":
                        _context.Users.Remove(user);
                        break;
                    case "deleteUnverified":
                        if (user.Status == UserStatus.Unverified)
                        {
                            _context.Users.Remove(user);
                        }
                        break;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Action completed successfully" });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim.Value);
        }
    }
}
