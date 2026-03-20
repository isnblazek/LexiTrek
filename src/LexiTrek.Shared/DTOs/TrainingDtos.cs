namespace LexiTrek.Shared.DTOs;

public record StartSessionDto(int Mode, Guid? GroupId, Guid? TagId);

public record SessionDto(
    Guid Id,
    int Mode,
    Guid? GroupId,
    Guid? TagId,
    DateTime StartedAt,
    bool IsCompleted
);

public record CompleteSessionDto(List<TrainingResultDto> Results);
public record TrainingResultDto(Guid WordId, int Result, DateTime AnsweredAt);

public record TrainingWordDto(
    Guid Id,
    string Term,
    string Definition,
    string? Notes,
    Guid GroupId,
    string GroupName
);

public record SessionResultsDto(
    Guid SessionId,
    int TotalWords,
    int RedCount,
    int OrangeCount,
    int GreenCount,
    DateTime StartedAt,
    DateTime CompletedAt,
    List<WordResultDto> Results
);

public record WordResultDto(
    Guid WordId,
    string Term,
    string Definition,
    int Result
);
