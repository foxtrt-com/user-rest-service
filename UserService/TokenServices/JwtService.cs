using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using UserService.Models;

namespace UserService.TokenServices;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly RSA _rsa;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;

        // Jwt config validation
        if (string.IsNullOrEmpty(_configuration["Jwt:PrivateKey"])
            || string.IsNullOrEmpty(_configuration["Jwt:Issuer"])
            || string.IsNullOrEmpty(_configuration["Jwt:Audience"])
            || string.IsNullOrEmpty(_configuration["Jwt:ExpirationMinutes"])
            || string.IsNullOrEmpty(_configuration["Jwt:RefreshTokenExpirationDays"])
        )
        {
            Console.WriteLine("JWT configuration is missing or incomplete. JWT Service cannot be used.");
            return;
        }

        // Setup RSA for asymetric token encryption
        _rsa = RSA.Create();
        _rsa.ImportFromPem(_configuration["Jwt:PrivateKey"]!);
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Setup claims for the token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Add user roles into claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Build the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpirationMinutes"]!)),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new RsaSecurityKey(_rsa),
                SecurityAlgorithms.RsaSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        Console.WriteLine($"Access token generated for user {user.Id}");

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // Setup token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(_rsa),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromMinutes(1) // Allow 1 minute clock skew
            };

            // Validate token and return claims principal
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Ensure the token uses expected encryption algorithm
            if (validatedToken is JwtSecurityToken jwtToken &&
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"Token validation failed: invalid algorithm {jwtToken.Header.Alg}");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return null;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        // Validate token without checking expiration
        return ValidateToken(token, false);
    }
}
