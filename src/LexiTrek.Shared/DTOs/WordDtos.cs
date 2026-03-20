namespace LexiTrek.Shared.DTOs;

public record CreateWordDto(string Term, string Definition, string? Notes);
public record UpdateWordDto(string Term, string Definition, string? Notes);

public record WordDto(
    Guid Id,
    Guid GroupId,
    string Term,
    string Definition,
    string? Notes,
    List<TagDto> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
