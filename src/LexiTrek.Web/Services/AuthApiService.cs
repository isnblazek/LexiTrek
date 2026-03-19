using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class AuthApiService
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;
    private readonly JwtAuthStateProvider _authState;

    public AuthApiService(HttpClient http, TokenStorageService tokenStorage, JwtAuthStateProvider authState)
    {
        _http = http;
        _tokenStorage = tokenStorage;
        _authState = authState;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", dto);
        if (!response.IsSuccessStatusCode) return false;

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokens == null) return false;

        await _tokenStorage.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken);
        _authState.NotifyAuthenticationStateChanged();
        return true;
    }

    public async Task<bool> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return false;

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokens == null) return false;

        await _tokenStorage.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken);
        _authState.NotifyAuthenticationStateChanged();
        return true;
    }

    public async Task LogoutAsync()
    {
        await _tokenStorage.ClearTokensAsync();
        _authState.NotifyAuthenticationStateChanged();
    }
}
