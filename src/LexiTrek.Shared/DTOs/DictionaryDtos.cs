namespace LexiTrek.Shared.DTOs;

public record CreateDictionaryDto(int SourceLanguage, int TargetLanguage);

public record DictionaryDto(
    Guid Id,
    int SourceLanguage,
    int TargetLanguage,
    string? OwnerId,
    int Visibility,
    DateTime CreatedAt
);

public record DictionaryListDto(
    Guid Id,
    int SourceLanguage,
    int TargetLanguage
);
