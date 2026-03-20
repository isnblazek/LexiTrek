using LexiTrek.Domain.Entities;
using LexiTrek.Domain.Enums;
using LexiTrek.Infrastructure;
using LexiTrek.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("http://localhost:5038", "https://localhost:5038")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed default dictionary
    var defaultDictionaryId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    if (!await db.Dictionaries.AnyAsync(d => d.Id == defaultDictionaryId))
    {
        db.Dictionaries.Add(new Dictionary
        {
            Id = defaultDictionaryId,
            SourceLanguage = "Čeština",
            TargetLanguage = "Angličtina",
            OwnerId = null,
            Visibility = Visibility.Public,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseWebAssemblyDebugging();
}

app.UseCors("AllowBlazor");
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
