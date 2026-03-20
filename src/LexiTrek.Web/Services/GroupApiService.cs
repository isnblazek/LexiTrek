using System.Net.Http.Json;
using LexiTrek.Shared.DTOs;

namespace LexiTrek.Web.Services;

public class GroupApiService
{
    private readonly HttpClient _http;

    public GroupApiService(HttpClient http) => _http = http;

    public async Task<List<GroupListDto>> GetUserGroupsAsync()
        => await _http.GetFromJsonAsync<List<GroupListDto>>("api/groups") ?? [];

    public async Task<GroupDto?> GetGroupAsync(Guid id)
        => await _http.GetFromJsonAsync<GroupDto>($"api/groups/{id}");

    public async Task<GroupDto?> CreateGroupAsync(CreateGroupDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/groups", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GroupDto>()
            : null;
    }

    public async Task<GroupDto?> UpdateGroupAsync(Guid id, UpdateGroupDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/groups/{id}", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GroupDto>()
            : null;
    }

    public async Task<bool> DeleteGroupAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/groups/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<PagedResult<GroupListDto>?> GetPublicGroupsAsync(string? search, int page = 1, Guid? dictionaryId = null)
    {
        var url = $"api/groups/public?page={page}&pageSize=20";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (dictionaryId.HasValue) url += $"&dictionaryId={dictionaryId.Value}";
        return await _http.GetFromJsonAsync<PagedResult<GroupListDto>>(url);
    }

    public async Task<bool> SubscribeAsync(Guid groupId)
    {
        var response = await _http.PostAsync($"api/groups/{groupId}/subscribe", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnsubscribeAsync(Guid groupId)
    {
        var response = await _http.DeleteAsync($"api/groups/{groupId}/subscribe");
        return response.IsSuccessStatusCode;
    }
}
