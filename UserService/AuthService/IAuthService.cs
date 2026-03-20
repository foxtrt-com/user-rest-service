using UserService.Models;

namespace UserService.AuthService;

public interface IAuthService
{
    Task<AuthResponse?> AuthenticateAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}
