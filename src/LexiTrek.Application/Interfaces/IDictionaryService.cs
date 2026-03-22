using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IDictionaryService
{
    Task<List<DictionaryListDto>> GetDictionariesAsync(string userId);
    Task<DictionaryListDto> CreateDictionaryAsync(CreateDictionaryDto dto, string userId);
    Task DeleteDictionaryAsync(long id, string userId);
}
