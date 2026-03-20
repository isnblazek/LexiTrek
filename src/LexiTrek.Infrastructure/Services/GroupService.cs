using LexiTrek.Application.Interfaces;
using LexiTrek.Domain.Entities;
using LexiTrek.Domain.Enums;
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
        var owned = await _db.WordGroups
            .Where(g => g.OwnerId == userId)
            .Include(g => g.Dictionary)
            .Select(g => new GroupListDto(
                g.Id, g.Name, g.Description,
                g.Owner.DisplayName, (int)g.Visibility, g.Words.Count,
                g.DictionaryId, g.Dictionary.SourceLanguage, g.Dictionary.TargetLanguage))
            .ToListAsync();

        var subscribed = await _db.GroupSubscriptions
            .Where(s => s.UserId == userId)
            .Select(s => new GroupListDto(
                s.Group.Id, s.Group.Name, s.Group.Description,
                s.Group.Owner.DisplayName, (int)s.Group.Visibility, s.Group.Words.Count,
                s.Group.DictionaryId, s.Group.Dictionary.SourceLanguage, s.Group.Dictionary.TargetLanguage))
            .ToListAsync();

        return [.. owned, .. subscribed];
    }

    public async Task<GroupDto> GetGroupAsync(Guid id, string userId)
    {
        var group = await _db.WordGroups
            .Include(g => g.Owner)
            .Include(g => g.Words)
            .Include(g => g.Dictionary)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException("Group not found");

        if (group.Visibility == Visibility.Private && group.OwnerId != userId)
        {
            var isSubscribed = await _db.GroupSubscriptions
                .AnyAsync(s => s.GroupId == id && s.UserId == userId);
            if (!isSubscribed)
                throw new UnauthorizedAccessException("Access denied");
        }

        return new GroupDto(
            group.Id, group.Name, group.Description,
            group.OwnerId, group.Owner.DisplayName,
            (int)group.Visibility, group.Words.Count,
            group.DictionaryId, group.Dictionary.SourceLanguage, group.Dictionary.TargetLanguage,
            group.CreatedAt, group.UpdatedAt);
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, string userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var dictionary = await _db.Dictionaries.FindAsync(dto.DictionaryId)
            ?? throw new KeyNotFoundException("Dictionary not found");

        var group = new WordGroup
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId,
            DictionaryId = dto.DictionaryId,
            Visibility = (Visibility)dto.Visibility,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.WordGroups.Add(group);
        await _db.SaveChangesAsync();

        return new GroupDto(
            group.Id, group.Name, group.Description,
            group.OwnerId, user.DisplayName,
            (int)group.Visibility, 0,
            group.DictionaryId, dictionary.SourceLanguage, dictionary.TargetLanguage,
            group.CreatedAt, group.UpdatedAt);
    }

    public async Task<GroupDto> UpdateGroupAsync(Guid id, UpdateGroupDto dto, string userId)
    {
        var group = await _db.WordGroups
            .Include(g => g.Owner)
            .Include(g => g.Words)
            .Include(g => g.Dictionary)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException("Group not found");

        if (group.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the owner can update this group");

        group.Name = dto.Name;
        group.Description = dto.Description;
        group.Visibility = (Visibility)dto.Visibility;
        group.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new GroupDto(
            group.Id, group.Name, group.Description,
            group.OwnerId, group.Owner.DisplayName,
            (int)group.Visibility, group.Words.Count,
            group.DictionaryId, group.Dictionary.SourceLanguage, group.Dictionary.TargetLanguage,
            group.CreatedAt, group.UpdatedAt);
    }

    public async Task DeleteGroupAsync(Guid id, string userId)
    {
        var group = await _db.WordGroups.FindAsync(id)
            ?? throw new KeyNotFoundException("Group not found");

        if (group.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the owner can delete this group");

        _db.WordGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<GroupListDto>> GetPublicGroupsAsync(PublicGroupsRequest request)
    {
        var query = _db.WordGroups
            .Where(g => g.Visibility == Visibility.Public);

        if (request.DictionaryId.HasValue)
            query = query.Where(g => g.DictionaryId == request.DictionaryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(g =>
                g.Name.ToLower().Contains(search) ||
                (g.Description != null && g.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(g => new GroupListDto(
                g.Id, g.Name, g.Description,
                g.Owner.DisplayName, (int)g.Visibility, g.Words.Count,
                g.DictionaryId, g.Dictionary.SourceLanguage, g.Dictionary.TargetLanguage))
            .ToListAsync();

        return new PagedResult<GroupListDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task SubscribeAsync(Guid groupId, string userId)
    {
        var group = await _db.WordGroups.FindAsync(groupId)
            ?? throw new KeyNotFoundException("Group not found");

        if (group.Visibility != Visibility.Public)
            throw new InvalidOperationException("Can only subscribe to public groups");

        if (group.OwnerId == userId)
            throw new InvalidOperationException("Cannot subscribe to your own group");

        var existing = await _db.GroupSubscriptions
            .AnyAsync(s => s.GroupId == groupId && s.UserId == userId);
        if (existing)
            throw new InvalidOperationException("Already subscribed");

        _db.GroupSubscriptions.Add(new GroupSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = groupId,
            SubscribedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(Guid groupId, string userId)
    {
        var sub = await _db.GroupSubscriptions
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.UserId == userId)
            ?? throw new KeyNotFoundException("Subscription not found");

        _db.GroupSubscriptions.Remove(sub);
        await _db.SaveChangesAsync();
    }
}
