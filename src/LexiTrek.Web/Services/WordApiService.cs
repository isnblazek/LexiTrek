using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class WordApiService
{
    private readonly HttpClient _http;

    public WordApiService(HttpClient http) => _http = http;

    public async Task<List<DictionaryEntryDto>> GetEntriesAsync(long dictionaryId)
        => await _http.GetFromJsonAsync<List<DictionaryEntryDto>>($"api/dictionaries/{dictionaryId}/entries") ?? [];

    public async Task<List<DictionaryEntryDto>> GetEntriesByGroupAsync(long groupId)
        => await _http.GetFromJsonAsync<List<DictionaryEntryDto>>($"api/groups/{groupId}/entries") ?? [];

    public async Task<DictionaryEntryDto?> AddEntryAsync(long dictionaryId, CreateEntryDto dto, long? groupId = null)
    {
        var url = $"api/dictionaries/{dictionaryId}/entries";
        if (groupId.HasValue) url += $"?groupId={groupId}";
        var response = await _http.PostAsJsonAsync(url, dto);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<DictionaryEntryDto>() : null;
    }

    public async Task<DictionaryEntryDto?> UpdateEntryAsync(long entryId, UpdateEntryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/entries/{entryId}", dto);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<DictionaryEntryDto>() : null;
    }

    public async Task<bool> RemoveEntryAsync(long entryId)
    {
        var response = await _http.DeleteAsync($"api/entries/{entryId}");
        return response.IsSuccessStatusCode;
    }
}
