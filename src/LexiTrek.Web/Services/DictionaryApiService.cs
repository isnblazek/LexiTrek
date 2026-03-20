using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class DictionaryApiService
{
    private readonly HttpClient _http;

    public DictionaryApiService(HttpClient http) => _http = http;

    public async Task<List<DictionaryListDto>> GetDictionariesAsync()
        => await _http.GetFromJsonAsync<List<DictionaryListDto>>("api/dictionaries") ?? [];

    public async Task<DictionaryDto?> GetDictionaryAsync(Guid id)
        => await _http.GetFromJsonAsync<DictionaryDto>($"api/dictionaries/{id}");

    public async Task<DictionaryDto?> CreateDictionaryAsync(CreateDictionaryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/dictionaries", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<DictionaryDto>()
            : null;
    }

    public async Task<bool> DeleteDictionaryAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/dictionaries/{id}");
        return response.IsSuccessStatusCode;
    }
}
