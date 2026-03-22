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

    [HttpGet("api/dictionaries/{dictionaryId:long}/entries")]
    public async Task<ActionResult<List<DictionaryEntryDto>>> GetEntries(long dictionaryId)
    {
        try { return Ok(await _wordService.GetEntriesAsync(dictionaryId, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("api/groups/{groupId:long}/entries")]
    public async Task<ActionResult<List<DictionaryEntryDto>>> GetEntriesByGroup(long groupId)
    {
        try { return Ok(await _wordService.GetEntriesByGroupAsync(groupId, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("api/dictionaries/{dictionaryId:long}/entries")]
    public async Task<ActionResult<DictionaryEntryDto>> AddEntry(long dictionaryId, CreateEntryDto dto, [FromQuery] long? groupId = null)
    {
        try
        {
            var result = await _wordService.AddEntryAsync(dictionaryId, dto, UserId, groupId);
            return Created($"api/entries/{result.Id}", result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("api/entries/{id:long}")]
    public async Task<ActionResult<DictionaryEntryDto>> UpdateEntry(long id, UpdateEntryDto dto)
    {
        try { return Ok(await _wordService.UpdateEntryAsync(id, dto, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("api/entries/{id:long}")]
    public async Task<ActionResult> RemoveEntry(long id)
    {
        try { await _wordService.RemoveEntryAsync(id, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
