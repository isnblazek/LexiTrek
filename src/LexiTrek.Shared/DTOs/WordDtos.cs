namespace LexiTrek.Shared.DTOs;

public record CreateEntryDto(string SourceText, string TargetText, string? Notes);
public record UpdateEntryDto(string SourceText, string TargetText, string? Notes);

public record DictionaryEntryDto(
    long Id,
    long WordPairId,
    string SourceText,
    string TargetText,
    string? Notes,
    bool IsActive,
    List<TagDto> Tags
);
