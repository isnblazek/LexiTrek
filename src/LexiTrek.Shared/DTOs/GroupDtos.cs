namespace LexiTrek.Shared.DTOs;

public record CreateGroupDto(string Name, string? Description, int Visibility);
public record UpdateGroupDto(string Name, string? Description, int Visibility);

public record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    string OwnerId,
    string OwnerDisplayName,
    int Visibility,
    int WordCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GroupListDto(
    Guid Id,
    string Name,
    string? Description,
    string OwnerDisplayName,
    int Visibility,
    int WordCount
);

public record PublicGroupsRequest(string? Search, int Page = 1, int PageSize = 20);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
