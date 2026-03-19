namespace LexiTrek.Shared.DTOs;

public record RegisterDto(string Email, string UserName, string Password, string DisplayName);
public record LoginDto(string Email, string Password);
public record RefreshDto(string RefreshToken);

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
public record AuthUserDto(string Id, string Email, string UserName, string DisplayName);
