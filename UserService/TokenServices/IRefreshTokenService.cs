using UserService.Models;

namespace UserService.TokenServices;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();
    Task<RefreshToken> RotateRefreshTokenAsync(string oldToken);
    Task SaveRefreshTokenAsync(int userId, string refreshToken);
    Task<int> GetUserIdFromToken(string token);
    Task RevokeRefreshTokenAsync(string token);
}