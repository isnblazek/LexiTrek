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
        => Ok(await _groupService.GetUserGroupsAsync(UserId));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<GroupDto>> GetGroup(long id)
    {
        try { return Ok(await _groupService.GetGroupAsync(id, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto dto)
    {
        try { var result = await _groupService.CreateGroupAsync(dto, UserId); return CreatedAtAction(nameof(GetGroup), new { id = result.Id }, result); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<GroupDto>> UpdateGroup(long id, UpdateGroupDto dto)
    {
        try { return Ok(await _groupService.UpdateGroupAsync(id, dto, UserId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeleteGroup(long id)
    {
        try { await _groupService.DeleteGroupAsync(id, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("public")]
    public async Task<ActionResult<PagedResult<GroupListDto>>> GetPublicGroups(
        [FromQuery] string? search, [FromQuery] long? dictionaryId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _groupService.GetPublicGroupsAsync(new PublicGroupsRequest(search, dictionaryId, page, pageSize), UserId));
    }

    [HttpPost("{id:long}/fork")]
    public async Task<ActionResult<GroupDto>> ForkGroup(long id)
    {
        try { var result = await _groupService.ForkGroupAsync(id, UserId); return CreatedAtAction(nameof(GetGroup), new { id = result.Id }, result); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{groupId:long}/entries/{entryId:long}")]
    public async Task<ActionResult> AddEntryToGroup(long groupId, long entryId)
    {
        try { await _groupService.AddEntryToGroupAsync(groupId, entryId, UserId); return Ok(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{groupId:long}/entries/{entryId:long}")]
    public async Task<ActionResult> RemoveEntryFromGroup(long groupId, long entryId)
    {
        try { await _groupService.RemoveEntryFromGroupAsync(groupId, entryId, UserId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
