namespace UserService.TokenServices;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();
    Task<string> RotateRefreshTokenAsync(string oldToken, int userId);
    Task SaveRefreshTokenAsync(int userId, string refreshToken);
}
