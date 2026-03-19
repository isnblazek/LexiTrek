using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(RegisterDto dto);
    Task<TokenResponse> LoginAsync(LoginDto dto);
    Task<TokenResponse> RefreshAsync(string refreshToken);
}
