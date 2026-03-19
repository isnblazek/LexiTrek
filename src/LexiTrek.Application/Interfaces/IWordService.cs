using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IWordService
{
    Task<List<WordDto>> GetWordsAsync(Guid groupId, string userId);
    Task<WordDto> CreateWordAsync(Guid groupId, CreateWordDto dto, string userId);
    Task<WordDto> UpdateWordAsync(Guid wordId, UpdateWordDto dto, string userId);
    Task DeleteWordAsync(Guid wordId, string userId);
}
