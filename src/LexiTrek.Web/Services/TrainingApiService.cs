using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class TrainingApiService
{
    private readonly HttpClient _http;

    public TrainingApiService(HttpClient http) => _http = http;

    public async Task<List<TrainingWordDto>> GetTrainingWordsAsync(long? groupId, long? tagId, int count = 20)
    {
        var url = $"api/training/words?count={count}";
        if (groupId.HasValue) url += $"&groupId={groupId}";
        if (tagId.HasValue) url += $"&tagId={tagId}";
        return await _http.GetFromJsonAsync<List<TrainingWordDto>>(url) ?? [];
    }

    public async Task<SessionDto?> StartSessionAsync(StartSessionDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/training/sessions", dto);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SessionDto>() : null;
    }

    public async Task<SessionResultsDto?> CompleteSessionAsync(long sessionId, CompleteSessionDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/training/sessions/{sessionId}/complete", dto);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SessionResultsDto>() : null;
    }
}
