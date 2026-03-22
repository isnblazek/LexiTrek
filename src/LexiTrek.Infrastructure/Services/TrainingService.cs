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

    public async Task<List<TrainingWordDto>> GetTrainingWordsAsync(long? groupId, long? tagId, int count, string userId)
    {
        IQueryable<DictionaryEntry> entryQuery;

        if (groupId.HasValue)
        {
            var group = await _db.WordGroups.FindAsync(groupId.Value)
                ?? throw new KeyNotFoundException("Skupina nenalezena");

            if (!group.IsPublic && group.OwnerId != userId)
                throw new UnauthorizedAccessException("Přístup zamítnut");

            entryQuery = _db.DictionaryEntries
                .Where(e => e.GroupIds.Contains(groupId.Value) && e.IsActive);
        }
        else if (tagId.HasValue)
        {
            var tag = await _db.Tags.FindAsync(tagId.Value)
                ?? throw new KeyNotFoundException("Tag nenalezen");
            if (tag.OwnerId != userId) throw new UnauthorizedAccessException("Přístup zamítnut");

            entryQuery = _db.DictionaryEntryTags
                .Where(et => et.TagId == tagId.Value)
                .Select(et => et.Entry)
                .Where(e => e.IsActive);
        }
        else
        {
            throw new ArgumentException("Musí být zadán groupId nebo tagId");
        }

        var wordPairIds = await entryQuery.Select(e => e.WordPairId).Distinct().ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var progressMap = await _db.UserWordProgresses
            .Where(p => p.UserId == userId && wordPairIds.Contains(p.WordPairId))
            .ToDictionaryAsync(p => p.WordPairId);

        var newIds = wordPairIds.Where(id => !progressMap.ContainsKey(id)).ToList();
        var dueIds = progressMap
            .Where(kv => kv.Value.NextReview <= today)
            .OrderBy(kv => kv.Value.NextReview)
            .Select(kv => kv.Key).ToList();

        var selectedIds = newIds.Concat(dueIds).Take(count).ToList();

        if (groupId.HasValue)
        {
            var groupName = await _db.WordGroups.Where(g => g.Id == groupId.Value).Select(g => g.Name).FirstAsync();
            return await entryQuery
                .Where(e => selectedIds.Contains(e.WordPairId))
                .Include(e => e.WordPair).ThenInclude(wp => wp.SourceWord)
                .Include(e => e.WordPair).ThenInclude(wp => wp.TargetWord)
                .Select(e => new TrainingWordDto(
                    e.WordPairId, e.WordPair.SourceWord.Text, e.WordPair.TargetWord.Text,
                    e.Notes, groupId.Value, groupName))
                .ToListAsync();
        }

        return await entryQuery
            .Where(e => selectedIds.Contains(e.WordPairId))
            .Include(e => e.WordPair).ThenInclude(wp => wp.SourceWord)
            .Include(e => e.WordPair).ThenInclude(wp => wp.TargetWord)
            .Select(e => new TrainingWordDto(
                e.WordPairId, e.WordPair.SourceWord.Text, e.WordPair.TargetWord.Text,
                e.Notes, 0, ""))
            .ToListAsync();
    }

    public async Task<SessionDto> StartSessionAsync(StartSessionDto dto, string userId)
    {
        var session = new TrainingSession
        {
            UserId = userId, Mode = (TrainingMode)dto.Mode,
            GroupId = dto.GroupId, TagId = dto.TagId,
            StartedAt = DateTime.UtcNow, IsCompleted = false,
            ClientSessionId = Guid.NewGuid()
        };
        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();

        return new SessionDto(session.Id, (int)session.Mode, session.GroupId, session.TagId, session.StartedAt, session.IsCompleted);
    }

    public async Task<SessionResultsDto> CompleteSessionAsync(long sessionId, CompleteSessionDto dto, string userId)
    {
        var session = await _db.TrainingSessions.Include(s => s.Results).FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session nenalezena");
        if (session.UserId != userId) throw new UnauthorizedAccessException("Přístup zamítnut");
        if (session.IsCompleted) throw new InvalidOperationException("Session je již dokončena");

        foreach (var r in dto.Results)
        {
            session.Results.Add(new TrainingResult
            {
                SessionId = sessionId, WordPairId = r.WordPairId,
                Result = (TrainingResultType)r.Result, AnsweredAt = r.AnsweredAt
            });

            var progress = await _db.UserWordProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.WordPairId == r.WordPairId);

            if (progress == null)
            {
                progress = new UserWordProgress
                {
                    UserId = userId, WordPairId = r.WordPairId,
                    EaseFactor = 2.5, IntervalDays = 0, Repetitions = 0,
                    NextReview = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                _db.UserWordProgresses.Add(progress);
            }

            SpacedRepetitionService.UpdateProgress(progress, (TrainingResultType)r.Result);
        }

        session.IsCompleted = true;
        session.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var wordPairIds = dto.Results.Select(r => r.WordPairId).ToList();
        var wordPairs = await _db.WordPairs
            .Where(wp => wordPairIds.Contains(wp.Id))
            .Include(wp => wp.SourceWord).Include(wp => wp.TargetWord)
            .ToDictionaryAsync(wp => wp.Id);

        var wordResults = dto.Results.Select(r => new WordResultDto(
            r.WordPairId,
            wordPairs.TryGetValue(r.WordPairId, out var wp) ? wp.SourceWord.Text : "",
            wp?.TargetWord.Text ?? "", r.Result)).ToList();

        return new SessionResultsDto(
            session.Id, dto.Results.Count,
            dto.Results.Count(r => r.Result == 0), dto.Results.Count(r => r.Result == 1),
            dto.Results.Count(r => r.Result == 2),
            session.StartedAt, session.CompletedAt!.Value, wordResults);
    }
}
