using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UserService.AsyncDataServices;
using UserService.AuthService;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;

    public AuthController(
        IConfiguration configuration,
        IAuthService authService)
    {
        _configuration = configuration;
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<ActionResult> Login(LoginRequest loginRequest)
    {
        // Authenticate user and generate tokens
        var response = await _authService.AuthenticateAsync(loginRequest);

        // Return 401 if authentication fails
        if (response == null)
        {
            return Unauthorized();
        }

        // Set refresh token in HTTP only cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!))
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        // Return access token in response body without refresh token
        return Ok(new
        {
            accessToken = response.AccessToken,
            expiresAt = response.ExpiresAt,
            user = response.User
        });
    }

    [HttpPost("Refresh")]
    public async Task<ActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        // If no refresh token return 401
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var response = await _authService.RefreshTokenAsync(refreshToken);

        // If refresh token is invalid return 401
        if (response == null)
        {
            return Unauthorized();
        }

        // Set new refresh token in HTTP only cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!))
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        // Return access token in response body without refresh token
        return Ok(new
        {
            accessToken = response.AccessToken,
            expiresAt = response.ExpiresAt,
            user = response.User
        });
    }

    [HttpPost("Logout")]
    public async Task<ActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        // If refresh token is present, revoke it
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeTokenAsync(refreshToken);
        }

        // Remove the refresh token cookie
        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}