using LexiTrek.Infrastructure.Data;
using LexiTrek.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LexiTrek.Api.Controllers;

[ApiController]
[Route("api/languages")]
public class LanguagesController : ControllerBase
{
    private readonly AppDbContext _db;

    public LanguagesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<LanguageDto>>> GetLanguages()
    {
        var languages = await _db.Languages
            .OrderBy(l => l.Id)
            .Select(l => new LanguageDto(l.Id, l.Code, l.Name))
            .ToListAsync();

        return Ok(languages);
    }
}
