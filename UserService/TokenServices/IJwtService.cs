using System.Security.Claims;
using UserService.Models;

namespace UserService.TokenServices;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
