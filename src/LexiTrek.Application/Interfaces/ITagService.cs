using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetTagsAsync(string userId);
    Task<TagDto> CreateTagAsync(CreateTagDto dto, string userId);
    Task<TagDto> UpdateTagAsync(long id, UpdateTagDto dto, string userId);
    Task DeleteTagAsync(long id, string userId);
    Task AssignTagsAsync(long entryId, AssignTagsDto dto, string userId);
    Task RemoveTagAsync(long entryId, long tagId, string userId);
}
