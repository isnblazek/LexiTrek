namespace LexiTrek.Shared.DTOs;

public record CreateGroupDto(string Name, string? Description, long DictionaryId, bool IsPublic = false);
public record UpdateGroupDto(string Name, string? Description, bool IsPublic);

public record GroupDto(
    long Id,
    string Name,
    string? Description,
    string OwnerId,
    string OwnerDisplayName,
    bool IsPublic,
    int EntryCount,
    long DictionaryId,
    string SourceLangName,
    string TargetLangName,
    long? SourceGroupId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsOwner
);

public record GroupListDto(
    long Id,
    string Name,
    string? Description,
    string OwnerDisplayName,
    bool IsPublic,
    int EntryCount,
    long DictionaryId,
    string SourceLangName,
    string TargetLangName
);

public record PublicGroupsRequest(string? Search, long? DictionaryId = null, int Page = 1, int PageSize = 20);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
