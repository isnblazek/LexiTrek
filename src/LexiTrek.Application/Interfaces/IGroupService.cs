using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IGroupService
{
    Task<List<GroupListDto>> GetUserGroupsAsync(string userId);
    Task<GroupDto> GetGroupAsync(long id, string userId);
    Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, string userId);
    Task<GroupDto> UpdateGroupAsync(long id, UpdateGroupDto dto, string userId);
    Task DeleteGroupAsync(long id, string userId);
    Task<PagedResult<GroupListDto>> GetPublicGroupsAsync(PublicGroupsRequest request);
    Task<GroupDto> ForkGroupAsync(long sourceGroupId, string userId);
    Task AddEntryToGroupAsync(long groupId, long entryId, string userId);
    Task RemoveEntryFromGroupAsync(long groupId, long entryId, string userId);
}
