# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Jazyk

Projekt je česky. UI texty, komentáře, commit messages a komunikace s uživatelem jsou v češtině.

## Příkazy

```bash
# Databáze (PostgreSQL 17, vyžaduje Docker)
docker compose -f docker-compose.yml up -d

# Spuštění aplikace (Blazor WASM + ASP.NET Core API, port 5038)
dotnet run --project src/LexiTrek.Api --launch-profile http

# Build
dotnet build

# Testy (xUnit)
dotnet test                                          # všechny testy
dotnet test tests/LexiTrek.Tests                       # jeden projekt
dotnet test --filter "FullyQualifiedName~EntityKey"  # jeden test/třída

# EF Core migrace
dotnet ef migrations add <Name> --project src/LexiTrek.Infrastructure --startup-project src/LexiTrek.Api
dotnet ef database update --project src/LexiTrek.Infrastructure --startup-project src/LexiTrek.Api
```

## Architektura

Clean Architecture — Blazor WebAssembly frontend hostovaný přes ASP.NET Core API (6 src + 1 test projekt):

| Projekt | Role |
|---------|------|
| `LexiTrek.Api` | ASP.NET Core host — API controllery, servíruje WASM frontend, JWT autentizace |
| `LexiTrek.Web` | Blazor WebAssembly frontend — MudBlazor UI, klientská autentizace |
| `LexiTrek.Application` | Use cases, business logika, rozhraní služeb (IAuthService, IDictionaryService…) |
| `LexiTrek.Domain` | Doménové entity, žádné závislosti |
| `LexiTrek.Infrastructure` | EF Core DbContext, ASP.NET Identity, implementace služeb |
| `LexiTrek.Shared` | Sdílené DTO a enumy (používá Web i Application) |
| `LexiTrek.Tests` | xUnit testy pro Domain a Application |

## Deploy (lokální vývoj)

Po úpravě kódu je nutné restartovat běžící aplikaci, aby se změny projevily:

1. **Zastavit** běžící proces (kill na portu 5038)
2. **Spustit znovu**: `dotnet run --project src/LexiTrek.Api --launch-profile http`
3. **Refresh prohlížeče**

Alternativně použít `dotnet watch` pro automatický hot reload:
```bash
dotnet watch run --project src/LexiTrek.Api --launch-profile http
```

**Důležité:** Při spouštění vždy používat `--no-restore` u buildu, pokud nejsou změny v NuGet balíčcích – `dotnet restore` může viset na síti. Aplikace běží na **http://localhost:5038**.

**Důležité:** Před buildem (`dotnet build`) je nutné nejdřív zastavit běžící aplikaci (`kill` procesu na portu 5038). Běžící aplikace zamyká soubory a build pak visí nebo selže. Postup pro vývoj: kill → build → spuštění.

## UI vzory (MudBlazor)

**Formulářová validace:**
- `<MudForm @ref="_form">` obaluje pole
- `Required="true"` na povinných polích
- V submit: `await _form.Validate(); if (!_form.IsValid) return;`
- Nepoužívat `Disabled` na tlačítku ani ruční `if (string.IsNullOrWhiteSpace(...))` kontroly

**Dialogy (MudDialog):**
- `DialogParameters<T>` pro typované parametry
- `DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true }` pro šířku
- `[CascadingParameter] IMudDialogInstance MudDialog` v dialog komponentě
- `MudDialog.Close(DialogResult.Ok(...))` / `MudDialog.Cancel()`

**Snackbary:**
- `Snackbar.Add("...", Severity.Success)` pro notifikace (texty hardcoded česky)

**MudButtonGroup (gotcha):**
- `MudButtonGroup` s `Variant` přepisuje individuální `Variant` na child `MudButton` — nelze mít různé varianty per button
- Pro toggle tlačítka s různými variantami použít samostatné `MudButton` s `border-radius` + `margin-left:-1px` pro vizuální spojení

## Autentizace

- **ASP.NET Identity** s vlastním `AppUser` entity
- **JWT Bearer** — access token (15 min), refresh token (7 dní)
- Konfigurace: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` v appsettings
- Refresh tokeny uloženy jako shadow properties na `AppUser` (EF Core)
- **Klient (WASM):** `TokenStorageService` ukládá tokeny do `localStorage`, `JwtAuthStateProvider` parsuje JWT claims, `AuthHeaderHandler` přidává Bearer header

## Klíčové konvence

- **.NET 10**, C# latest, nullable enabled, implicit usings
- **PostgreSQL 17** s `pg_trgm` rozšířením pro fulltext
