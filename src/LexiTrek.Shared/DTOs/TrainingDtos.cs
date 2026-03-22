namespace LexiTrek.Shared.DTOs;

public record StartSessionDto(int Mode, long? GroupId, long? TagId);

public record SessionDto(
    long Id,
    int Mode,
    long? GroupId,
    long? TagId,
    DateTime StartedAt,
    bool IsCompleted
);

public record CompleteSessionDto(List<TrainingResultDto> Results);
public record TrainingResultDto(long WordPairId, int Result, DateTime AnsweredAt);

public record TrainingWordDto(
    long WordPairId,
    string SourceText,
    string TargetText,
    string? Notes,
    long GroupId,
    string GroupName
);

public record SessionResultsDto(
    long SessionId,
    int TotalWords,
    int RedCount,
    int OrangeCount,
    int GreenCount,
    DateTime StartedAt,
    DateTime CompletedAt,
    List<WordResultDto> Results
);

public record WordResultDto(
    long WordPairId,
    string SourceText,
    string TargetText,
    int Result
);
