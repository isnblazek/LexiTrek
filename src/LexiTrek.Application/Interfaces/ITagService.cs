using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetTagsAsync(string userId);
    Task<TagDto> CreateTagAsync(CreateTagDto dto, string userId);
    Task<TagDto> UpdateTagAsync(Guid id, UpdateTagDto dto, string userId);
    Task DeleteTagAsync(Guid id, string userId);
    Task AssignTagsAsync(Guid wordId, AssignTagsDto dto, string userId);
    Task RemoveTagAsync(Guid wordId, Guid tagId, string userId);
}
