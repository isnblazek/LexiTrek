using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface ITrainingService
{
    Task<List<TrainingWordDto>> GetTrainingWordsAsync(long? groupId, long? tagId, int count, string userId, string? filter = null);
    Task<SessionDto> StartSessionAsync(StartSessionDto dto, string userId);
    Task<SessionResultsDto> CompleteSessionAsync(long sessionId, CompleteSessionDto dto, string userId);
    Task<TrainingStatsDto> GetTrainingStatsAsync(long? dictionaryId, string userId);
    Task<List<ErrorEntryDto>> GetErrorEntriesAsync(long? dictionaryId, string userId);
}
