using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class TagApiService
{
    private readonly HttpClient _http;

    public TagApiService(HttpClient http) => _http = http;

    public async Task<List<TagDto>> GetTagsAsync()
        => await _http.GetFromJsonAsync<List<TagDto>>("api/tags") ?? [];

    public async Task<TagDto?> CreateTagAsync(CreateTagDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/tags", dto);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TagDto>() : null;
    }

    public async Task<TagDto?> UpdateTagAsync(long id, UpdateTagDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/tags/{id}", dto);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TagDto>() : null;
    }

    public async Task<bool> DeleteTagAsync(long id)
    {
        var response = await _http.DeleteAsync($"api/tags/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AssignTagsAsync(long entryId, AssignTagsDto dto)
    {
        var response = await _http.PostAsJsonAsync($"api/entries/{entryId}/tags", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveTagAsync(long entryId, long tagId)
    {
        var response = await _http.DeleteAsync($"api/entries/{entryId}/tags/{tagId}");
        return response.IsSuccessStatusCode;
    }
}
