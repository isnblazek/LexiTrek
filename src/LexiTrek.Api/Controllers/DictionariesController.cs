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

    [HttpPost]
    public async Task<ActionResult<DictionaryListDto>> CreateDictionary(CreateDictionaryDto dto)
    {
        try
        {
            var result = await _dictionaryService.CreateDictionaryAsync(dto, UserId);
            return Created($"api/dictionaries/{result.Id}", result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeleteDictionary(long id)
    {
        try
        {
            await _dictionaryService.DeleteDictionaryAsync(id, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
