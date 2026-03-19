using LexiTrek.Application.Interfaces;
using LexiTrek.Application.Services;
using LexiTrek.Domain.Entities;
using LexiTrek.Domain.Enums;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Services;

public class TrainingService : ITrainingService
{
    private readonly AppDbContext _db;

    public TrainingService(AppDbContext db) => _db = db;

    public async Task<List<TrainingWordDto>> GetTrainingWordsAsync(Guid? groupId, Guid? tagId, int count, string userId)
    {
        IQueryable<Word> wordQuery;

        if (groupId.HasValue)
        {
            var group = await _db.WordGroups.FindAsync(groupId.Value)
                ?? throw new KeyNotFoundException("Group not found");

            if (group.Visibility == Visibility.Private && group.OwnerId != userId)
            {
                var isSub = await _db.GroupSubscriptions
                    .AnyAsync(s => s.GroupId == groupId.Value && s.UserId == userId);
                if (!isSub) throw new UnauthorizedAccessException("Access denied");
            }

            wordQuery = _db.Words.Where(w => w.GroupId == groupId.Value);
        }
        else if (tagId.HasValue)
        {
            var tag = await _db.Tags.FindAsync(tagId.Value)
                ?? throw new KeyNotFoundException("Tag not found");
            if (tag.OwnerId != userId)
                throw new UnauthorizedAccessException("Access denied");

            wordQuery = _db.WordTags
                .Where(wt => wt.TagId == tagId.Value)
                .Select(wt => wt.Word);
        }
        else
        {
            throw new ArgumentException("Either groupId or tagId must be provided");
        }

        var wordIds = await wordQuery.Select(w => w.Id).ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var progressMap = await _db.WordProgresses
            .Where(p => p.UserId == userId && wordIds.Contains(p.WordId))
            .ToDictionaryAsync(p => p.WordId);

        // Priority 1: New words (no progress)
        var newWordIds = wordIds.Where(id => !progressMap.ContainsKey(id)).ToList();

        // Priority 2: Words due for review
        var dueWordIds = progressMap
            .Where(kv => kv.Value.NextReviewDate <= today)
            .OrderBy(kv => kv.Value.NextReviewDate)
            .Select(kv => kv.Key)
            .ToList();

        var selectedIds = newWordIds.Concat(dueWordIds).Take(count).ToList();

        return await _db.Words
            .Where(w => selectedIds.Contains(w.Id))
            .Include(w => w.Group)
            .Select(w => new TrainingWordDto(
                w.Id, w.Czech, w.English, w.Notes,
                w.GroupId, w.Group.Name))
            .ToListAsync();
    }

    public async Task<SessionDto> StartSessionAsync(StartSessionDto dto, string userId)
    {
        var session = new TrainingSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Mode = (TrainingMode)dto.Mode,
            GroupId = dto.GroupId,
            TagId = dto.TagId,
            StartedAt = DateTime.UtcNow,
            IsCompleted = false,
            ClientSessionId = Guid.NewGuid()
        };

        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();

        return new SessionDto(
            session.Id, (int)session.Mode, session.GroupId,
            session.TagId, session.StartedAt, session.IsCompleted);
    }

    public async Task<SessionResultsDto> CompleteSessionAsync(Guid sessionId, CompleteSessionDto dto, string userId)
    {
        var session = await _db.TrainingSessions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found");

        if (session.UserId != userId)
            throw new UnauthorizedAccessException("Access denied");

        if (session.IsCompleted)
            throw new InvalidOperationException("Session already completed");

        // Save results
        foreach (var r in dto.Results)
        {
            session.Results.Add(new TrainingResult
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                WordId = r.WordId,
                Result = (TrainingResultType)r.Result,
                AnsweredAt = r.AnsweredAt
            });

            // Update spaced repetition progress
            var progress = await _db.WordProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == r.WordId);

            if (progress == null)
            {
                progress = new WordProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    WordId = r.WordId,
                    EaseFactor = 2.5,
                    IntervalDays = 0,
                    RepetitionCount = 0,
                    NextReviewDate = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                _db.WordProgresses.Add(progress);
            }

            SpacedRepetitionService.UpdateProgress(progress, (TrainingResultType)r.Result);
        }

        session.IsCompleted = true;
        session.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Build response
        var wordIds = dto.Results.Select(r => r.WordId).ToList();
        var words = await _db.Words
            .Where(w => wordIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id);

        var wordResults = dto.Results.Select(r => new WordResultDto(
            r.WordId,
            words.TryGetValue(r.WordId, out var w) ? w.Czech : "",
            w?.English ?? "",
            r.Result
        )).ToList();

        return new SessionResultsDto(
            session.Id,
            dto.Results.Count,
            dto.Results.Count(r => r.Result == 0),
            dto.Results.Count(r => r.Result == 1),
            dto.Results.Count(r => r.Result == 2),
            session.StartedAt,
            session.CompletedAt!.Value,
            wordResults);
    }
}
