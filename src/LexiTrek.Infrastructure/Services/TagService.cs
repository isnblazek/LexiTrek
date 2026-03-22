using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly AppDbContext _db;

    public TagService(AppDbContext db) => _db = db;

    public async Task<List<TagDto>> GetTagsAsync(string userId)
        => await _db.Tags.Where(t => t.OwnerId == userId).Select(t => new TagDto(t.Id, t.Name)).ToListAsync();

    public async Task<TagDto> CreateTagAsync(CreateTagDto dto, string userId)
    {
        if (await _db.Tags.AnyAsync(t => t.OwnerId == userId && t.Name == dto.Name))
            throw new InvalidOperationException("Tag s tímto názvem již existuje");

        var tag = new Tag { Name = dto.Name, OwnerId = userId, CreatedAt = DateTime.UtcNow };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return new TagDto(tag.Id, tag.Name);
    }

    public async Task<TagDto> UpdateTagAsync(long id, UpdateTagDto dto, string userId)
    {
        var tag = await _db.Tags.FindAsync(id) ?? throw new KeyNotFoundException("Tag nenalezen");
        if (tag.OwnerId != userId) throw new UnauthorizedAccessException("Přístup zamítnut");
        if (await _db.Tags.AnyAsync(t => t.OwnerId == userId && t.Name == dto.Name && t.Id != id))
            throw new InvalidOperationException("Tag s tímto názvem již existuje");

        tag.Name = dto.Name;
        await _db.SaveChangesAsync();
        return new TagDto(tag.Id, tag.Name);
    }

    public async Task DeleteTagAsync(long id, string userId)
    {
        var tag = await _db.Tags.FindAsync(id) ?? throw new KeyNotFoundException("Tag nenalezen");
        if (tag.OwnerId != userId) throw new UnauthorizedAccessException("Přístup zamítnut");
        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
    }

    public async Task AssignTagsAsync(long entryId, AssignTagsDto dto, string userId)
    {
        var entry = await _db.DictionaryEntries
            .Include(e => e.Dictionary)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == entryId)
            ?? throw new KeyNotFoundException("Záznam nenalezen");

        if (entry.Dictionary.UserId != userId)
            throw new UnauthorizedAccessException("Pouze vlastník slovníku může přidávat tagy");

        var userTags = await _db.Tags
            .Where(t => t.OwnerId == userId && dto.TagIds.Contains(t.Id))
            .Select(t => t.Id).ToListAsync();

        foreach (var tagId in userTags)
        {
            if (!entry.Tags.Any(et => et.TagId == tagId))
                entry.Tags.Add(new DictionaryEntryTag { DictionaryEntryId = entryId, DictionaryId = entry.DictionaryId, TagId = tagId });
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoveTagAsync(long entryId, long tagId, string userId)
    {
        var entryTag = await _db.DictionaryEntryTags
            .Include(et => et.Entry).ThenInclude(e => e.Dictionary)
            .FirstOrDefaultAsync(et => et.DictionaryEntryId == entryId && et.TagId == tagId)
            ?? throw new KeyNotFoundException("Přiřazení tagu nebylo nalezeno");

        if (entryTag.Entry.Dictionary.UserId != userId)
            throw new UnauthorizedAccessException("Přístup zamítnut");

        _db.DictionaryEntryTags.Remove(entryTag);
        await _db.SaveChangesAsync();
    }
}
