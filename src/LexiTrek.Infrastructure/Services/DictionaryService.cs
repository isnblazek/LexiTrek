using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Domain.Enums;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Services;

public class DictionaryService : IDictionaryService
{
    private readonly AppDbContext _db;

    public DictionaryService(AppDbContext db) => _db = db;

    public async Task<List<DictionaryListDto>> GetDictionariesAsync(string userId)
    {
        return await _db.Dictionaries
            .Where(d => d.OwnerId == null || d.OwnerId == userId || d.Visibility == Visibility.Public)
            .Select(d => new DictionaryListDto(d.Id, d.SourceLanguage, d.TargetLanguage))
            .ToListAsync();
    }

    public async Task<DictionaryDto> GetDictionaryAsync(Guid id)
    {
        var d = await _db.Dictionaries.FindAsync(id)
            ?? throw new KeyNotFoundException("Dictionary not found");

        return new DictionaryDto(d.Id, d.SourceLanguage, d.TargetLanguage, d.OwnerId, (int)d.Visibility, d.CreatedAt);
    }

    public async Task<DictionaryDto> CreateDictionaryAsync(CreateDictionaryDto dto, string userId)
    {
        var dictionary = new Dictionary
        {
            Id = Guid.NewGuid(),
            SourceLanguage = dto.SourceLanguage,
            TargetLanguage = dto.TargetLanguage,
            OwnerId = userId,
            Visibility = Visibility.Private,
            CreatedAt = DateTime.UtcNow
        };

        _db.Dictionaries.Add(dictionary);
        await _db.SaveChangesAsync();

        return new DictionaryDto(
            dictionary.Id, dictionary.SourceLanguage, dictionary.TargetLanguage,
            dictionary.OwnerId, (int)dictionary.Visibility, dictionary.CreatedAt);
    }

    public async Task DeleteDictionaryAsync(Guid id, string userId)
    {
        var dictionary = await _db.Dictionaries.FindAsync(id)
            ?? throw new KeyNotFoundException("Dictionary not found");

        if (dictionary.OwnerId == null)
            throw new InvalidOperationException("Cannot delete system dictionary");

        if (dictionary.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the owner can delete this dictionary");

        var hasGroups = await _db.WordGroups.AnyAsync(g => g.DictionaryId == id);
        if (hasGroups)
            throw new InvalidOperationException("Cannot delete dictionary with existing groups");

        _db.Dictionaries.Remove(dictionary);
        await _db.SaveChangesAsync();
    }
}
