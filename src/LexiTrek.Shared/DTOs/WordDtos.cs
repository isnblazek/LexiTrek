namespace LexiTrek.Shared.DTOs;

public record CreateWordDto(string Czech, string English, string? Notes);
public record UpdateWordDto(string Czech, string English, string? Notes);

public record WordDto(
    Guid Id,
    Guid GroupId,
    string Czech,
    string English,
    string? Notes,
    List<TagDto> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
