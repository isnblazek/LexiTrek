using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext _db;

    public GroupService(AppDbContext db) => _db = db;

    public async Task<List<GroupListDto>> GetUserGroupsAsync(string userId)
    {
        var groups = await _db.WordGroups
            .Where(g => g.OwnerId == userId)
            .Include(g => g.Owner)
            .Include(g => g.Dictionary).ThenInclude(d => d.SourceLang)
            .Include(g => g.Dictionary).ThenInclude(d => d.TargetLang)
            .ToListAsync();

        var result = new List<GroupListDto>();
        foreach (var g in groups)
        {
            var entryCount = await _db.DictionaryEntries
                .CountAsync(e => e.DictionaryId == g.DictionaryId && e.IsActive && e.GroupIds.Contains(g.Id));

            result.Add(new GroupListDto(
                g.Id, g.Name, g.Description,
                g.Owner.DisplayName, g.IsPublic, entryCount,
                g.DictionaryId, g.Dictionary.SourceLang.Name, g.Dictionary.TargetLang.Name));
        }

        return result;
    }

    public async Task<GroupDto> GetGroupAsync(long id, string userId)
    {
        var group = await _db.WordGroups
            .Include(g => g.Owner)
            .Include(g => g.Dictionary).ThenInclude(d => d.SourceLang)
            .Include(g => g.Dictionary).ThenInclude(d => d.TargetLang)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException("Skupina nenalezena");

        if (!group.IsPublic && group.OwnerId != userId)
            throw new UnauthorizedAccessException("Přístup zamítnut");

        var entryCount = await _db.DictionaryEntries
            .CountAsync(e => e.DictionaryId == group.DictionaryId && e.IsActive && e.GroupIds.Contains(id));

        return new GroupDto(
            group.Id, group.Name, group.Description,
            group.OwnerId, group.Owner.DisplayName,
            group.IsPublic, entryCount,
            group.DictionaryId, group.Dictionary.SourceLang.Name, group.Dictionary.TargetLang.Name,
            group.SourceGroupId, group.CreatedAt, group.UpdatedAt,
            IsOwner: group.OwnerId == userId);
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, string userId)
    {
        var dictionary = await _db.Dictionaries
            .Include(d => d.SourceLang).Include(d => d.TargetLang)
            .FirstOrDefaultAsync(d => d.Id == dto.DictionaryId && d.UserId == userId)
            ?? throw new KeyNotFoundException("Slovník nenalezen nebo nepatří uživateli");

        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Uživatel nenalezen");

        var group = new WordGroup
        {
            Name = dto.Name, Description = dto.Description,
            OwnerId = userId, DictionaryId = dto.DictionaryId,
            IsPublic = dto.IsPublic,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        _db.WordGroups.Add(group);
        await _db.SaveChangesAsync();

        return new GroupDto(
            group.Id, group.Name, group.Description,
            group.OwnerId, user.DisplayName, group.IsPublic, 0,
            group.DictionaryId, dictionary.SourceLang.Name, dictionary.TargetLang.Name,
            null, group.CreatedAt, group.UpdatedAt, IsOwner: true);
    }

    public async Task<GroupDto> UpdateGroupAsync(long id, UpdateGroupDto dto, string userId)
    {
        var group = await _db.WordGroups
            .Include(g => g.Owner)
            .Include(g => g.Dictionary).ThenInclude(d => d.SourceLang)
            .Include(g => g.Dictionary).ThenInclude(d => d.TargetLang)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException("Skupina nenalezena");

        if (group.OwnerId != userId)
            throw new UnauthorizedAccessException("Pouze vlastník může upravovat skupinu");

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.IsPublic = dto.IsPublic;
        group.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var entryCount = await _db.DictionaryEntries
            .CountAsync(e => e.DictionaryId == group.DictionaryId && e.IsActive && e.GroupIds.Contains(id));

        return new GroupDto(
            group.Id, group.Name, group.Description,
            group.OwnerId, group.Owner.DisplayName, group.IsPublic, entryCount,
            group.DictionaryId, group.Dictionary.SourceLang.Name, group.Dictionary.TargetLang.Name,
            group.SourceGroupId, group.CreatedAt, group.UpdatedAt, IsOwner: true);
    }

    public async Task DeleteGroupAsync(long id, string userId)
    {
        var group = await _db.WordGroups.FindAsync(id) ?? throw new KeyNotFoundException("Skupina nenalezena");
        if (group.OwnerId != userId) throw new UnauthorizedAccessException("Pouze vlastník může smazat skupinu");

        // Odebrat groupId z GroupIds všech entries
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""DictionaryEntries""
            SET ""GroupIds"" = array_remove(""GroupIds"", {id})
            WHERE ""DictionaryId"" = {group.DictionaryId} AND {id} = ANY(""GroupIds"")");

        _db.WordGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<GroupListDto>> GetPublicGroupsAsync(PublicGroupsRequest request, string userId)
    {
        var query = _db.WordGroups.Where(g => g.IsPublic && g.OwnerId != userId);

        if (request.DictionaryId.HasValue)
        {
            var dict = await _db.Dictionaries.FindAsync(request.DictionaryId.Value);
            if (dict != null)
                query = query.Where(g => g.Dictionary.SourceLangId == dict.SourceLangId && g.Dictionary.TargetLangId == dict.TargetLangId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(g => g.Name.ToLower().Contains(search) ||
                (g.Description != null && g.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var groups = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(g => g.Owner)
            .Include(g => g.Dictionary).ThenInclude(d => d.SourceLang)
            .Include(g => g.Dictionary).ThenInclude(d => d.TargetLang)
            .ToListAsync();

        var items = new List<GroupListDto>();
        foreach (var g in groups)
        {
            var entryCount = await _db.DictionaryEntries
                .CountAsync(e => e.IsActive && e.GroupIds.Contains(g.Id));
            items.Add(new GroupListDto(
                g.Id, g.Name, g.Description,
                g.Owner.DisplayName, g.IsPublic, entryCount,
                g.DictionaryId, g.Dictionary.SourceLang.Name, g.Dictionary.TargetLang.Name));
        }

        return new PagedResult<GroupListDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<GroupDto> ForkGroupAsync(long sourceGroupId, string userId)
    {
        var source = await _db.WordGroups
            .Include(g => g.Dictionary).ThenInclude(d => d.SourceLang)
            .Include(g => g.Dictionary).ThenInclude(d => d.TargetLang)
            .FirstOrDefaultAsync(g => g.Id == sourceGroupId)
            ?? throw new KeyNotFoundException("Zdrojová skupina nenalezena");

        if (!source.IsPublic) throw new InvalidOperationException("Lze forkovat pouze veřejné skupiny");
        if (source.OwnerId == userId) throw new InvalidOperationException("Nelze forkovat vlastní skupinu");

        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Uživatel nenalezen");

        var dictionary = await _db.Dictionaries.FirstOrDefaultAsync(d =>
            d.UserId == userId && d.SourceLangId == source.Dictionary.SourceLangId && d.TargetLangId == source.Dictionary.TargetLangId);

        if (dictionary == null)
        {
            dictionary = new Dictionary
            {
                UserId = userId, SourceLangId = source.Dictionary.SourceLangId,
                TargetLangId = source.Dictionary.TargetLangId, CreatedAt = DateTime.UtcNow
            };
            _db.Dictionaries.Add(dictionary);
            await _db.SaveChangesAsync();
        }

        var fork = new WordGroup
        {
            Name = source.Name, Description = source.Description,
            OwnerId = userId, DictionaryId = dictionary.Id,
            IsPublic = false, SourceGroupId = sourceGroupId,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.WordGroups.Add(fork);
        await _db.SaveChangesAsync();

        // Pro každé slovíčko: find-or-create entry + přidat fork.Id do GroupIds
        var sourceEntries = await _db.DictionaryEntries
            .Where(e => e.GroupIds.Contains(sourceGroupId) && e.IsActive)
            .ToListAsync();

        foreach (var sourceEntry in sourceEntries)
        {
            var myEntry = await _db.DictionaryEntries
                .FirstOrDefaultAsync(e => e.DictionaryId == dictionary.Id && e.WordPairId == sourceEntry.WordPairId);

            if (myEntry == null)
            {
                myEntry = new DictionaryEntry
                {
                    DictionaryId = dictionary.Id, WordPairId = sourceEntry.WordPairId,
                    IsActive = true, Notes = sourceEntry.Notes, GroupIds = [fork.Id]
                };
                _db.DictionaryEntries.Add(myEntry);
                await _db.SaveChangesAsync();

                // Zkopírovat tagy
                var sourceTags = await _db.DictionaryEntryTags
                    .Where(et => et.DictionaryEntryId == sourceEntry.Id && et.DictionaryId == sourceEntry.DictionaryId)
                    .Include(et => et.Tag)
                    .ToListAsync();

                foreach (var st in sourceTags)
                {
                    var myTag = await _db.Tags.FirstOrDefaultAsync(t => t.OwnerId == userId && t.Name == st.Tag.Name);
                    if (myTag == null)
                    {
                        myTag = new Tag { Name = st.Tag.Name, OwnerId = userId, CreatedAt = DateTime.UtcNow };
                        _db.Tags.Add(myTag);
                        await _db.SaveChangesAsync();
                    }
                    _db.DictionaryEntryTags.Add(new DictionaryEntryTag
                    {
                        DictionaryEntryId = myEntry.Id, DictionaryId = dictionary.Id, TagId = myTag.Id
                    });
                }
                await _db.SaveChangesAsync();
            }
            else
            {
                if (!myEntry.IsActive) myEntry.IsActive = true;
                if (!myEntry.GroupIds.Contains(fork.Id))
                    myEntry.GroupIds = [.. myEntry.GroupIds, fork.Id];
                await _db.SaveChangesAsync();
            }
        }

        var entryCount = await _db.DictionaryEntries
            .CountAsync(e => e.DictionaryId == dictionary.Id && e.IsActive && e.GroupIds.Contains(fork.Id));

        return new GroupDto(
            fork.Id, fork.Name, fork.Description,
            fork.OwnerId, user.DisplayName, fork.IsPublic, entryCount,
            fork.DictionaryId, source.Dictionary.SourceLang.Name, source.Dictionary.TargetLang.Name,
            fork.SourceGroupId, fork.CreatedAt, fork.UpdatedAt, IsOwner: true);
    }

    public async Task AddEntryToGroupAsync(long groupId, long entryId, string userId)
    {
        var group = await _db.WordGroups.FindAsync(groupId) ?? throw new KeyNotFoundException("Skupina nenalezena");
        if (group.OwnerId != userId) throw new UnauthorizedAccessException("Pouze vlastník může přidávat do skupiny");

        var entry = await _db.DictionaryEntries.FirstOrDefaultAsync(e => e.Id == entryId)
            ?? throw new KeyNotFoundException("Slovíčko nenalezeno");

        if (!entry.GroupIds.Contains(groupId))
        {
            entry.GroupIds = [.. entry.GroupIds, groupId];
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveEntryFromGroupAsync(long groupId, long entryId, string userId)
    {
        var group = await _db.WordGroups.FindAsync(groupId) ?? throw new KeyNotFoundException("Skupina nenalezena");
        if (group.OwnerId != userId) throw new UnauthorizedAccessException("Pouze vlastník může odebírat ze skupiny");

        var entry = await _db.DictionaryEntries.FirstOrDefaultAsync(e => e.Id == entryId)
            ?? throw new KeyNotFoundException("Slovíčko nenalezeno");

        if (entry.GroupIds.Contains(groupId))
        {
            entry.GroupIds = entry.GroupIds.Where(id => id != groupId).ToArray();
            await _db.SaveChangesAsync();
        }
    }
}
