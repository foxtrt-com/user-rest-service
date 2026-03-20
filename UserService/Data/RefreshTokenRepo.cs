using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class RefreshTokenRepo : IRefreshTokenRepo
{
    private readonly AppDbContext _context;

    public RefreshTokenRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(RefreshToken token)
    {
        // Check if token is null, throw exception if it is
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        // Add token to Db
        await _context.RefreshTokens.AddAsync(token);
    }

    public async Task<RefreshToken?> GetTokenAsync(string token)
    {
        // Return refresh token object that matches the token string
        return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        // Get all tokens for the user
        var userTokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();

        // Revoke each token by setting IsRevoked to true and RevokedAt to current time
        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        // Save changes to Db, return true if changes > 0 were saved
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving refresh token: {ex.Message}");
            return false;
        }

    }

    public void Update(RefreshToken token)
    {
        // Check if token is null, throw exception if it is
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        // Update token in Db
        _context.RefreshTokens.Update(token);
    }
}
