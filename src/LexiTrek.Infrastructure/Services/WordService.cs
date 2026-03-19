using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Domain.Enums;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Services;

public class WordService : IWordService
{
    private readonly AppDbContext _db;

    public WordService(AppDbContext db) => _db = db;

    public async Task<List<WordDto>> GetWordsAsync(Guid groupId, string userId)
    {
        var group = await _db.WordGroups.FindAsync(groupId)
            ?? throw new KeyNotFoundException("Group not found");

        if (group.Visibility == Visibility.Private && group.OwnerId != userId)
        {
            var isSubscribed = await _db.GroupSubscriptions
                .AnyAsync(s => s.GroupId == groupId && s.UserId == userId);
            if (!isSubscribed)
                throw new UnauthorizedAccessException("Access denied");
        }

        return await _db.Words
            .Where(w => w.GroupId == groupId)
            .Select(w => new WordDto(
                w.Id, w.GroupId, w.Czech, w.English, w.Notes,
                w.WordTags.Select(wt => new TagDto(wt.Tag.Id, wt.Tag.Name)).ToList(),
                w.CreatedAt, w.UpdatedAt))
            .ToListAsync();
    }

    public async Task<WordDto> CreateWordAsync(Guid groupId, CreateWordDto dto, string userId)
    {
        var group = await _db.WordGroups.FindAsync(groupId)
            ?? throw new KeyNotFoundException("Group not found");

        if (group.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the owner can add words");

        var word = new Word
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Czech = dto.Czech,
            English = dto.English,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Words.Add(word);
        await _db.SaveChangesAsync();

        return new WordDto(word.Id, word.GroupId, word.Czech, word.English, word.Notes, [], word.CreatedAt, word.UpdatedAt);
    }

    public async Task<WordDto> UpdateWordAsync(Guid wordId, UpdateWordDto dto, string userId)
    {
        var word = await _db.Words
            .Include(w => w.Group)
            .Include(w => w.WordTags).ThenInclude(wt => wt.Tag)
            .FirstOrDefaultAsync(w => w.Id == wordId)
            ?? throw new KeyNotFoundException("Word not found");

        if (word.Group.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the group owner can update words");

        word.Czech = dto.Czech;
        word.English = dto.English;
        word.Notes = dto.Notes;
        word.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new WordDto(
            word.Id, word.GroupId, word.Czech, word.English, word.Notes,
            word.WordTags.Select(wt => new TagDto(wt.Tag.Id, wt.Tag.Name)).ToList(),
            word.CreatedAt, word.UpdatedAt);
    }

    public async Task DeleteWordAsync(Guid wordId, string userId)
    {
        var word = await _db.Words
            .Include(w => w.Group)
            .FirstOrDefaultAsync(w => w.Id == wordId)
            ?? throw new KeyNotFoundException("Word not found");

        if (word.Group.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the group owner can delete words");

        _db.Words.Remove(word);
        await _db.SaveChangesAsync();
    }
}
