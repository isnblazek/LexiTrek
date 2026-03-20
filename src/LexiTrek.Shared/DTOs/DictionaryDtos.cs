namespace LexiTrek.Shared.DTOs;

public record CreateDictionaryDto(string SourceLanguage, string TargetLanguage);

public record DictionaryDto(
    Guid Id,
    string SourceLanguage,
    string TargetLanguage,
    string? OwnerId,
    int Visibility,
    DateTime CreatedAt
);

public record DictionaryListDto(
    Guid Id,
    string SourceLanguage,
    string TargetLanguage
);
