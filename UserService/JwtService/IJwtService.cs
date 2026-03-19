using System.Security.Claims;
using UserService.Models;

namespace UserService.JwtService;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
