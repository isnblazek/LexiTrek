using System.Security.Claims;
using LexiTrek.Application.Interfaces;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiTrek.Api.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService) => _tagService = tagService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetTags() => Ok(await _tagService.GetTagsAsync(UserId));

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto dto)
    {
        try { return Created("api/tags", await _tagService.CreateTagAsync(dto, UserId)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<TagDto>> UpdateTag(long id, UpdateTagDto dto)
    {
        try { return Ok(await _tagService.UpdateTagAsync(id, dto, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeleteTag(long id)
    {
        try { await _tagService.DeleteTagAsync(id, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("~/api/entries/{entryId:long}/tags")]
    public async Task<ActionResult> AssignTags(long entryId, AssignTagsDto dto)
    {
        try { await _tagService.AssignTagsAsync(entryId, dto, UserId); return Ok(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("~/api/entries/{entryId:long}/tags/{tagId:long}")]
    public async Task<ActionResult> RemoveTag(long entryId, long tagId)
    {
        try { await _tagService.RemoveTagAsync(entryId, tagId, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
