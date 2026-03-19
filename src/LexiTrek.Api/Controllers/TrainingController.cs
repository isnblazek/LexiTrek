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

    [HttpGet("words")]
    public async Task<ActionResult<List<TrainingWordDto>>> GetTrainingWords(
        [FromQuery] Guid? groupId, [FromQuery] Guid? tagId, [FromQuery] int count = 20)
    {
        try
        {
            return Ok(await _trainingService.GetTrainingWordsAsync(groupId, tagId, count, UserId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<SessionDto>> StartSession(StartSessionDto dto)
    {
        var result = await _trainingService.StartSessionAsync(dto, UserId);
        return Created($"api/training/sessions/{result.Id}", result);
    }

    [HttpPut("sessions/{id:guid}/complete")]
    public async Task<ActionResult<SessionResultsDto>> CompleteSession(Guid id, CompleteSessionDto dto)
    {
        try
        {
            return Ok(await _trainingService.CompleteSessionAsync(id, dto, UserId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
