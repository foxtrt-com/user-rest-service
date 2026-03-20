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
    private readonly IRefreshTokenRepo _refreshTokenRepo;

    public AuthService(
        IConfiguration configuration,
        IJwtService jwtService,
        IUserRepo userRepo,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepo refreshTokenRepo
    )
    {
        _configuration = configuration;
        _jwtService = jwtService;
        _userRepo = userRepo;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepo = refreshTokenRepo;
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
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:Duration"]!)),
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
        var token = await _refreshTokenRepo.GetTokenAsync(refreshToken);
        var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken, token.UserId);

        var user = _userRepo.GetUserById(token.UserId);
        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:Duration"]!)),
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

    public Task<bool> RevokeTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}
