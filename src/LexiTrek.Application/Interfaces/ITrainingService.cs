using LexiTrek.Shared.DTOs;

namespace LexiTrek.Application.Interfaces;

public interface ITrainingService
{
    Task<List<TrainingWordDto>> GetTrainingWordsAsync(Guid? groupId, Guid? tagId, int count, string userId);
    Task<SessionDto> StartSessionAsync(StartSessionDto dto, string userId);
    Task<SessionResultsDto> CompleteSessionAsync(Guid sessionId, CompleteSessionDto dto, string userId);
}
