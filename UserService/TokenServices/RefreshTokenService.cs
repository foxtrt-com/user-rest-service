using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.Models;

namespace UserService.TokenServices;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepo _repo;

    public RefreshTokenService(IConfiguration configuration, IRefreshTokenRepo repo)
    {
        _configuration = configuration;
        _repo = repo;
    }

    public string GenerateRefreshToken()
    {
        //Generate random 64 byte refresh token converted to base64
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<string> RotateRefreshTokenAsync(string oldToken, int userId)
    {
        // Validate the old token exists and belongs to the user
        var existingToken = await _repo.GetTokenAsync(oldToken);

        if (existingToken == null || existingToken.UserId != userId)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // Check if token has already been used (potential attack)
        if (existingToken.IsUsed)
        {
            Console.WriteLine($"Refresh token reuse detected for user {userId}. Revoking all tokens");
            await _repo.RevokeAllUserTokensAsync(userId);
            await _repo.SaveChangesAsync();
            throw new SecurityTokenException("Token reuse detected. Please log in again.");
        }

        // Mark old token as used
        existingToken.IsUsed = true;
        existingToken.UsedAt = DateTime.UtcNow;
        _repo.Update(existingToken);
        await _repo.SaveChangesAsync();

        // Generate and save refresh token
        var newToken = GenerateRefreshToken();
        await SaveRefreshTokenAsync(userId, newToken);

        return newToken;
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        // Save refresh token to the database
        await _repo.CreateAsync(new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!)),
            CreatedAt = DateTime.UtcNow
        });
        await _repo.SaveChangesAsync();
    }
}
