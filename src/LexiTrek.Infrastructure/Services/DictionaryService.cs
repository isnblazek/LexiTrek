using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
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
            .Where(d => d.UserId == userId)
            .Include(d => d.SourceLang)
            .Include(d => d.TargetLang)
            .Select(d => new DictionaryListDto(
                d.Id, d.SourceLangId, d.TargetLangId,
                d.SourceLang.Name, d.TargetLang.Name))
            .ToListAsync();
    }

    public async Task<DictionaryListDto> CreateDictionaryAsync(CreateDictionaryDto dto, string userId)
    {
        var dictionary = new Dictionary
        {
            UserId = userId,
            SourceLangId = dto.SourceLangId,
            TargetLangId = dto.TargetLangId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Dictionaries.Add(dictionary);
        await _db.SaveChangesAsync();

        var sourceLang = await _db.Languages.FindAsync(dto.SourceLangId);
        var targetLang = await _db.Languages.FindAsync(dto.TargetLangId);

        return new DictionaryListDto(
            dictionary.Id, dictionary.SourceLangId, dictionary.TargetLangId,
            sourceLang!.Name, targetLang!.Name);
    }

    public async Task DeleteDictionaryAsync(long id, string userId)
    {
        var dictionary = await _db.Dictionaries.FindAsync(id)
            ?? throw new KeyNotFoundException("Slovník nebyl nalezen");

        if (dictionary.UserId != userId)
            throw new UnauthorizedAccessException("Pouze vlastník může smazat slovník");

        var hasGroups = await _db.WordGroups.AnyAsync(g => g.DictionaryId == id);
        if (hasGroups)
            throw new InvalidOperationException("Nelze smazat slovník se skupinami");

        _db.Dictionaries.Remove(dictionary);
        await _db.SaveChangesAsync();
    }
}
