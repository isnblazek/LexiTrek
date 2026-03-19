using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class WordApiService
{
    private readonly HttpClient _http;

    public WordApiService(HttpClient http) => _http = http;

    public async Task<List<WordDto>> GetWordsAsync(Guid groupId)
        => await _http.GetFromJsonAsync<List<WordDto>>($"api/groups/{groupId}/words") ?? [];

    public async Task<WordDto?> CreateWordAsync(Guid groupId, CreateWordDto dto)
    {
        var response = await _http.PostAsJsonAsync($"api/groups/{groupId}/words", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WordDto>()
            : null;
    }

    public async Task<WordDto?> UpdateWordAsync(Guid wordId, UpdateWordDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/words/{wordId}", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WordDto>()
            : null;
    }

    public async Task<bool> DeleteWordAsync(Guid wordId)
    {
        var response = await _http.DeleteAsync($"api/words/{wordId}");
        return response.IsSuccessStatusCode;
    }
}
