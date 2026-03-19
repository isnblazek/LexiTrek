using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LexiTrek.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(UserManager<AppUser> userManager, AppDbContext db, IConfiguration config)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.UserName,
            DisplayName = dto.DisplayName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return GenerateTokens(user);
    }

    public async Task<TokenResponse> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid credentials");

        return GenerateTokens(user);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => EF.Property<string>(u, "RefreshToken") == refreshToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var expiry = _db.Entry(user).Property<DateTime?>("RefreshTokenExpiry").CurrentValue;
        if (expiry == null || expiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        return GenerateTokens(user);
    }

    private TokenResponse GenerateTokens(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim("display_name", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        // Store refresh token (using shadow properties)
        _db.Entry(user).Property<string>("RefreshToken").CurrentValue = refreshToken;
        _db.Entry(user).Property<DateTime?>("RefreshTokenExpiry").CurrentValue = refreshExpiry;
        _db.SaveChanges();

        return new TokenResponse(accessToken, refreshToken, expiresAt);
    }
}
