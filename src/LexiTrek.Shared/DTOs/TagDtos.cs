namespace LexiTrek.Shared.DTOs;

public record CreateTagDto(string Name);
public record UpdateTagDto(string Name);
public record TagDto(Guid Id, string Name);
public record AssignTagsDto(List<Guid> TagIds);
