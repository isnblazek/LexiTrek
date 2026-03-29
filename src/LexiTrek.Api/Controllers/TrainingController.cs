using System.Security.Claims;
using LexiTrek.Application.Interfaces;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiTrek.Api.Controllers;

[ApiController]
[Route("api/training")]
[Authorize]
public class TrainingController : ControllerBase
{
    private readonly ITrainingService _trainingService;

    public TrainingController(ITrainingService trainingService) => _trainingService = trainingService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("stats")]
    public async Task<ActionResult<TrainingStatsDto>> GetStats([FromQuery] long? dictionaryId)
    {
        return Ok(await _trainingService.GetTrainingStatsAsync(dictionaryId, UserId));
    }

    [HttpGet("error-entries")]
    public async Task<ActionResult<List<ErrorEntryDto>>> GetErrorEntries([FromQuery] long? dictionaryId)
    {
        return Ok(await _trainingService.GetErrorEntriesAsync(dictionaryId, UserId));
    }

    [HttpGet("new-entries")]
    public async Task<ActionResult<List<NewEntryDto>>> GetNewEntries([FromQuery] long? dictionaryId)
    {
        return Ok(await _trainingService.GetNewEntriesAsync(dictionaryId, UserId));
    }

    [HttpGet("new-entries/{dictionaryId:long}")]
    public async Task<ActionResult<List<DictionaryEntryDto>>> GetNewDictionaryEntries(long dictionaryId)
    {
        return Ok(await _trainingService.GetNewDictionaryEntriesAsync(dictionaryId, UserId));
    }

    [HttpGet("words")]
    public async Task<ActionResult<List<TrainingWordDto>>> GetTrainingWords(
        [FromQuery] long? groupId, [FromQuery] long? tagId, [FromQuery] int count = 20, [FromQuery] string? filter = null)
    {
        try { return Ok(await _trainingService.GetTrainingWordsAsync(groupId, tagId, count, UserId, filter)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<SessionDto>> StartSession(StartSessionDto dto)
    {
        var result = await _trainingService.StartSessionAsync(dto, UserId);
        return Created($"api/training/sessions/{result.Id}", result);
    }

    [HttpPut("sessions/{id:long}/complete")]
    public async Task<ActionResult<SessionResultsDto>> CompleteSession(long id, CompleteSessionDto dto)
    {
        try { return Ok(await _trainingService.CompleteSessionAsync(id, dto, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
