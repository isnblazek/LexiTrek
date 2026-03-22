using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Services;

public class WordService : IWordService
{
    private readonly AppDbContext _db;

    public WordService(AppDbContext db) => _db = db;

    public async Task<List<DictionaryEntryDto>> GetEntriesAsync(long dictionaryId, string userId)
    {
        var dict = await _db.Dictionaries.FindAsync(dictionaryId)
            ?? throw new KeyNotFoundException("Slovník nenalezen");

        if (dict.UserId != userId)
            throw new UnauthorizedAccessException("Přístup zamítnut");

        return await BuildEntryQuery(_db.DictionaryEntries.Where(e => e.DictionaryId == dictionaryId && e.IsActive))
            .ToListAsync();
    }

    public async Task<List<DictionaryEntryDto>> GetEntriesByGroupAsync(long groupId, string userId)
    {
        var group = await _db.WordGroups.FindAsync(groupId)
            ?? throw new KeyNotFoundException("Skupina nenalezena");

        if (!group.IsPublic && group.OwnerId != userId)
            throw new UnauthorizedAccessException("Přístup zamítnut");

        return await BuildEntryQuery(_db.DictionaryEntries.Where(e => e.GroupIds.Contains(groupId) && e.IsActive))
            .ToListAsync();
    }

    public async Task<DictionaryEntryDto> AddEntryAsync(long dictionaryId, CreateEntryDto dto, string userId, long? groupId = null)
    {
        var dict = await _db.Dictionaries.FindAsync(dictionaryId)
            ?? throw new KeyNotFoundException("Slovník nenalezen");

        if (dict.UserId != userId)
            throw new UnauthorizedAccessException("Pouze vlastník může přidávat slovíčka");

        var sourceWord = await FindOrCreateWordAsync(dto.SourceText, dict.SourceLangId);
        var targetWord = await FindOrCreateWordAsync(dto.TargetText, dict.TargetLangId);
        var wordPair = await FindOrCreateWordPairAsync(sourceWord.Id, targetWord.Id);

        var existing = await _db.DictionaryEntries
            .FirstOrDefaultAsync(e => e.DictionaryId == dictionaryId && e.WordPairId == wordPair.Id);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.Notes = dto.Notes;
            }

            if (groupId.HasValue && !existing.GroupIds.Contains(groupId.Value))
                existing.GroupIds = [.. existing.GroupIds, groupId.Value];

            await _db.SaveChangesAsync();
            return await GetEntryDtoAsync(existing.Id, dictionaryId);
        }

        var entry = new DictionaryEntry
        {
            DictionaryId = dictionaryId,
            WordPairId = wordPair.Id,
            IsActive = true,
            Notes = dto.Notes,
            GroupIds = groupId.HasValue ? [groupId.Value] : []
        };

        _db.DictionaryEntries.Add(entry);
        await _db.SaveChangesAsync();

        return await GetEntryDtoAsync(entry.Id, dictionaryId);
    }

    public async Task<DictionaryEntryDto> UpdateEntryAsync(long entryId, UpdateEntryDto dto, string userId)
    {
        var entry = await _db.DictionaryEntries
            .Include(e => e.Dictionary)
            .FirstOrDefaultAsync(e => e.Id == entryId)
            ?? throw new KeyNotFoundException("Záznam nenalezen");

        if (entry.Dictionary.UserId != userId)
            throw new UnauthorizedAccessException("Pouze vlastník může upravovat slovíčka");

        var sourceWord = await FindOrCreateWordAsync(dto.SourceText, entry.Dictionary.SourceLangId);
        var targetWord = await FindOrCreateWordAsync(dto.TargetText, entry.Dictionary.TargetLangId);
        var wordPair = await FindOrCreateWordPairAsync(sourceWord.Id, targetWord.Id);

        entry.WordPairId = wordPair.Id;
        entry.Notes = dto.Notes;
        await _db.SaveChangesAsync();

        return await GetEntryDtoAsync(entry.Id, entry.DictionaryId);
    }

    public async Task RemoveEntryAsync(long entryId, string userId)
    {
        var entry = await _db.DictionaryEntries
            .Include(e => e.Dictionary)
            .FirstOrDefaultAsync(e => e.Id == entryId)
            ?? throw new KeyNotFoundException("Záznam nenalezen");

        if (entry.Dictionary.UserId != userId)
            throw new UnauthorizedAccessException("Pouze vlastník může mazat slovíčka");

        entry.IsActive = false;
        await _db.SaveChangesAsync();
    }

    private async Task<Word> FindOrCreateWordAsync(string text, int languageId)
    {
        var word = await _db.Words.FirstOrDefaultAsync(w => w.Text == text && w.LanguageId == languageId);
        if (word != null) return word;
        word = new Word { Text = text, LanguageId = languageId };
        _db.Words.Add(word);
        await _db.SaveChangesAsync();
        return word;
    }

    private async Task<WordPair> FindOrCreateWordPairAsync(long sourceWordId, long targetWordId)
    {
        var pair = await _db.WordPairs.FirstOrDefaultAsync(wp => wp.SourceWordId == sourceWordId && wp.TargetWordId == targetWordId);
        if (pair != null) return pair;
        pair = new WordPair { SourceWordId = sourceWordId, TargetWordId = targetWordId };
        _db.WordPairs.Add(pair);
        await _db.SaveChangesAsync();
        return pair;
    }

    private static IQueryable<DictionaryEntryDto> BuildEntryQuery(IQueryable<DictionaryEntry> query)
    {
        return query
            .Include(e => e.WordPair).ThenInclude(wp => wp.SourceWord)
            .Include(e => e.WordPair).ThenInclude(wp => wp.TargetWord)
            .Include(e => e.Tags).ThenInclude(et => et.Tag)
            .Select(e => new DictionaryEntryDto(
                e.Id, e.WordPairId,
                e.WordPair.SourceWord.Text, e.WordPair.TargetWord.Text,
                e.Notes, e.IsActive,
                e.Tags.Select(et => new TagDto(et.Tag.Id, et.Tag.Name)).ToList()));
    }

    private async Task<DictionaryEntryDto> GetEntryDtoAsync(long entryId, long dictionaryId)
    {
        return await BuildEntryQuery(_db.DictionaryEntries.Where(e => e.Id == entryId && e.DictionaryId == dictionaryId))
            .FirstAsync();
    }
}
