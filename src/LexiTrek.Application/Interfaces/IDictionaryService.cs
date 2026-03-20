using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IDictionaryService
{
    Task<List<DictionaryListDto>> GetDictionariesAsync(string userId);
    Task<DictionaryDto> GetDictionaryAsync(Guid id);
    Task<DictionaryDto> CreateDictionaryAsync(CreateDictionaryDto dto, string userId);
    Task DeleteDictionaryAsync(Guid id, string userId);
}
