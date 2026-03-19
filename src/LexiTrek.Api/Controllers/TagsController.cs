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
    public async Task<ActionResult<List<TagDto>>> GetTags()
    {
        return Ok(await _tagService.GetTagsAsync(UserId));
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto dto)
    {
        try
        {
            var result = await _tagService.CreateTagAsync(dto, UserId);
            return Created($"api/tags/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TagDto>> UpdateTag(Guid id, UpdateTagDto dto)
    {
        try
        {
            return Ok(await _tagService.UpdateTagAsync(id, dto, UserId));
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

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTag(Guid id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id, UserId);
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

    [HttpPost("~/api/words/{wordId:guid}/tags")]
    public async Task<ActionResult> AssignTags(Guid wordId, AssignTagsDto dto)
    {
        try
        {
            await _tagService.AssignTagsAsync(wordId, dto, UserId);
            return Ok();
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

    [HttpDelete("~/api/words/{wordId:guid}/tags/{tagId:guid}")]
    public async Task<ActionResult> RemoveTag(Guid wordId, Guid tagId)
    {
        try
        {
            await _tagService.RemoveTagAsync(wordId, tagId, UserId);
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
