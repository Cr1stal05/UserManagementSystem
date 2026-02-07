using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UserManagementSystem.Data;

namespace UserManagementSystem.Middleware
{
    public class UserCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;  // ← используйте IServiceScopeFactory

        public UserCheckMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;

            // Пропускаем публичные endpoints
            if (path.StartsWithSegments("/api/auth") ||
                path.StartsWithSegments("/swagger") ||
                path == "/")
            {
                await _next(context);
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            // Important: Создаем scope для DbContext
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var userId = Guid.Parse(userIdClaim.Value);
            var user = await dbContext.Users.FindAsync(userId);

            if (user == null || user.Status == Models.UserStatus.Blocked)
            {
                context.Response.Redirect("/login");
                return;
            }

            // Обновление времени активности
            user.LastActivityTime = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            await _next(context);
        }
    }
}