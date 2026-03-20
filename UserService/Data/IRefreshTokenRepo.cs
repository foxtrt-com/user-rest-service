using UserService.Models;

namespace UserService.Data;

public interface IRefreshTokenRepo
{
    Task<RefreshToken?> GetTokenAsync(string token);
    Task RevokeAllUserTokensAsync(int userId);
    void Update(RefreshToken token);
    Task CreateAsync(RefreshToken token);
    Task<bool> SaveChangesAsync();
}
