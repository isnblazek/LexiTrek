using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IGroupService
{
    Task<List<GroupListDto>> GetUserGroupsAsync(string userId);
    Task<GroupDto> GetGroupAsync(Guid id, string userId);
    Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, string userId);
    Task<GroupDto> UpdateGroupAsync(Guid id, UpdateGroupDto dto, string userId);
    Task DeleteGroupAsync(Guid id, string userId);
    Task<PagedResult<GroupListDto>> GetPublicGroupsAsync(PublicGroupsRequest request);
    Task SubscribeAsync(Guid groupId, string userId);
    Task UnsubscribeAsync(Guid groupId, string userId);
}
