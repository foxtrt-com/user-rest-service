using UserService.Dtos;
using UserService.TokenServices;
using UserService.Models;
using UserService.Data;

namespace UserService.AuthService;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly IUserRepo _userRepo;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IConfiguration configuration,
        IJwtService jwtService,
        IUserRepo userRepo,
        IRefreshTokenService refreshTokenService
    )
    {
        _configuration = configuration;
        _jwtService = jwtService;
        _userRepo = userRepo;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse?> AuthenticateAsync(LoginRequest request)
    {
        var user = _userRepo.GetUserByUsername(request.Username);

        //TODO: Validate password using hashing and salting

        if (user == null)
        {
            Console.WriteLine($"Authentication failed for user: {request.Username}");
            return null;
        }

        // Generate Jwt and refresh tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _refreshTokenService.GenerateRefreshToken();

        // Save refresh token
        await _refreshTokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"]!)),
            User = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles,
                CreatedAt = user.CreatedAt,
            }
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken);

        var user = _userRepo.GetUserById(newRefreshToken.UserId);
        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"]!)),
            User = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles,
                CreatedAt = user.CreatedAt,
            }
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);
    }
}
