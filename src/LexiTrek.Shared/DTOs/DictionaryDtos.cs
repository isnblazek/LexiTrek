namespace LexiTrek.Shared.DTOs;

public record CreateDictionaryDto(int SourceLangId, int TargetLangId);

public record DictionaryListDto(
    long Id,
    int SourceLangId,
    int TargetLangId,
    string SourceLangName,
    string TargetLangName,
    int EntryCount
);
