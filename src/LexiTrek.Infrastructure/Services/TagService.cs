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
    {
        return await _db.Tags
            .Where(t => t.OwnerId == userId)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToListAsync();
    }

    public async Task<TagDto> CreateTagAsync(CreateTagDto dto, string userId)
    {
        var exists = await _db.Tags
            .AnyAsync(t => t.OwnerId == userId && t.Name == dto.Name);
        if (exists)
            throw new InvalidOperationException("Tag with this name already exists");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        return new TagDto(tag.Id, tag.Name);
    }

    public async Task<TagDto> UpdateTagAsync(Guid id, UpdateTagDto dto, string userId)
    {
        var tag = await _db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException("Tag not found");

        if (tag.OwnerId != userId)
            throw new UnauthorizedAccessException("Access denied");

        var exists = await _db.Tags
            .AnyAsync(t => t.OwnerId == userId && t.Name == dto.Name && t.Id != id);
        if (exists)
            throw new InvalidOperationException("Tag with this name already exists");

        tag.Name = dto.Name;
        await _db.SaveChangesAsync();

        return new TagDto(tag.Id, tag.Name);
    }

    public async Task DeleteTagAsync(Guid id, string userId)
    {
        var tag = await _db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException("Tag not found");

        if (tag.OwnerId != userId)
            throw new UnauthorizedAccessException("Access denied");

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
    }

    public async Task AssignTagsAsync(Guid wordId, AssignTagsDto dto, string userId)
    {
        var word = await _db.Words
            .Include(w => w.Group)
            .Include(w => w.WordTags)
            .FirstOrDefaultAsync(w => w.Id == wordId)
            ?? throw new KeyNotFoundException("Word not found");

        if (word.Group.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the group owner can tag words");

        var userTags = await _db.Tags
            .Where(t => t.OwnerId == userId && dto.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        foreach (var tagId in userTags)
        {
            if (!word.WordTags.Any(wt => wt.TagId == tagId))
            {
                word.WordTags.Add(new WordTag { WordId = wordId, TagId = tagId });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoveTagAsync(Guid wordId, Guid tagId, string userId)
    {
        var wordTag = await _db.WordTags
            .Include(wt => wt.Word).ThenInclude(w => w.Group)
            .FirstOrDefaultAsync(wt => wt.WordId == wordId && wt.TagId == tagId)
            ?? throw new KeyNotFoundException("Tag assignment not found");

        if (wordTag.Word.Group.OwnerId != userId)
            throw new UnauthorizedAccessException("Access denied");

        _db.WordTags.Remove(wordTag);
        await _db.SaveChangesAsync();
    }
}
