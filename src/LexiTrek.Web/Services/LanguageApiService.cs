using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class LanguageApiService
{
    private readonly HttpClient _http;

    public LanguageApiService(HttpClient http) => _http = http;

    public async Task<List<LanguageDto>> GetLanguagesAsync()
        => await _http.GetFromJsonAsync<List<LanguageDto>>("api/languages") ?? [];
}
