using System.Security.Claims;

namespace Presentation.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Идентификатор неверный");
            }
            return userId;
        }

        public static string GetUserRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Роль отсутствует");

        }
    }
}
