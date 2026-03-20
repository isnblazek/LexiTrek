using System.Security.Claims;
using LexiTrek.Application.Interfaces;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiTrek.Api.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService) => _groupService = groupService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<GroupListDto>>> GetUserGroups()
    {
        return Ok(await _groupService.GetUserGroupsAsync(UserId));
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto dto)
    {
        var result = await _groupService.CreateGroupAsync(dto, UserId);
        return CreatedAtAction(nameof(GetGroup), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupDto>> GetGroup(Guid id)
    {
        try
        {
            return Ok(await _groupService.GetGroupAsync(id, UserId));
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GroupDto>> UpdateGroup(Guid id, UpdateGroupDto dto)
    {
        try
        {
            return Ok(await _groupService.UpdateGroupAsync(id, dto, UserId));
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

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteGroup(Guid id)
    {
        try
        {
            await _groupService.DeleteGroupAsync(id, UserId);
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

    [HttpGet("public")]
    public async Task<ActionResult<PagedResult<GroupListDto>>> GetPublicGroups(
        [FromQuery] string? search, [FromQuery] Guid? dictionaryId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var request = new PublicGroupsRequest(search, dictionaryId, page, pageSize);
        return Ok(await _groupService.GetPublicGroupsAsync(request));
    }

    [HttpPost("{id:guid}/subscribe")]
    public async Task<ActionResult> Subscribe(Guid id)
    {
        try
        {
            await _groupService.SubscribeAsync(id, UserId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/subscribe")]
    public async Task<ActionResult> Unsubscribe(Guid id)
    {
        try
        {
            await _groupService.UnsubscribeAsync(id, UserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
