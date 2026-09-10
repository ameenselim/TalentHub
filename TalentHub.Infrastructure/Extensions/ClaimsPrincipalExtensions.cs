
using System.Security.Claims;
using TalentHub.Infrastructure.Utilities;

namespace TalentHub.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier);

        public static string? GetFullName(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Name);

        public static bool IsSuperAdmin(this ClaimsPrincipal user)
            => user.IsInRole(SystemRoles.SUPER_ADMIN);

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole(SystemRoles.ADMIN);
    }
}
