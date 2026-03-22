using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface IWordService
{
    Task<List<DictionaryEntryDto>> GetEntriesAsync(long dictionaryId, string userId);
    Task<List<DictionaryEntryDto>> GetEntriesByGroupAsync(long groupId, string userId);
    Task<DictionaryEntryDto> AddEntryAsync(long dictionaryId, CreateEntryDto dto, string userId, long? groupId = null);
    Task<DictionaryEntryDto> UpdateEntryAsync(long entryId, UpdateEntryDto dto, string userId);
    Task RemoveEntryAsync(long entryId, string userId);
}
