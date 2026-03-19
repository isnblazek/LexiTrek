using System.Security.Claims;
using LexiTrek.Application.Interfaces;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiTrek.Api.Controllers;

[ApiController]
[Authorize]
public class WordsController : ControllerBase
{
    private readonly IWordService _wordService;

    public WordsController(IWordService wordService) => _wordService = wordService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("api/groups/{groupId:guid}/words")]
    public async Task<ActionResult<List<WordDto>>> GetWords(Guid groupId)
    {
        try
        {
            return Ok(await _wordService.GetWordsAsync(groupId, UserId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("api/groups/{groupId:guid}/words")]
    public async Task<ActionResult<WordDto>> CreateWord(Guid groupId, CreateWordDto dto)
    {
        try
        {
            var result = await _wordService.CreateWordAsync(groupId, dto, UserId);
            return Created($"api/words/{result.Id}", result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("api/words/{id:guid}")]
    public async Task<ActionResult<WordDto>> UpdateWord(Guid id, UpdateWordDto dto)
    {
        try
        {
            return Ok(await _wordService.UpdateWordAsync(id, dto, UserId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("api/words/{id:guid}")]
    public async Task<ActionResult> DeleteWord(Guid id)
    {
        try
        {
            await _wordService.DeleteWordAsync(id, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
