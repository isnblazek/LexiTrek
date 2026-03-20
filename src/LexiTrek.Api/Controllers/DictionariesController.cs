using System.Security.Claims;
using LexiTrek.Application.Interfaces;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiTrek.Api.Controllers;

[ApiController]
[Route("api/dictionaries")]
[Authorize]
public class DictionariesController : ControllerBase
{
    private readonly IDictionaryService _dictionaryService;

    public DictionariesController(IDictionaryService dictionaryService) => _dictionaryService = dictionaryService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<DictionaryListDto>>> GetDictionaries()
    {
        return Ok(await _dictionaryService.GetDictionariesAsync(UserId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DictionaryDto>> GetDictionary(Guid id)
    {
        try
        {
            return Ok(await _dictionaryService.GetDictionaryAsync(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<DictionaryDto>> CreateDictionary(CreateDictionaryDto dto)
    {
        var result = await _dictionaryService.CreateDictionaryAsync(dto, UserId);
        return CreatedAtAction(nameof(GetDictionary), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteDictionary(Guid id)
    {
        try
        {
            await _dictionaryService.DeleteDictionaryAsync(id, UserId);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
