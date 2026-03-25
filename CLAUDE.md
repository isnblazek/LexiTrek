# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Jazyk

Projekt je česky. UI texty, komentáře, commit messages a komunikace s uživatelem jsou v češtině.

## Specifikace aplikace

LexiTrek je webová aplikace pro učení slovíček metodou spaced repetition (SM-2).

### Doménový model

```
Uživatel
  └── Slovník (Dictionary) = jazykový pár (CZ↔EN), bez názvu
        ├── DictionaryEntry = slovíčko (→ WordPair → Word+Word), jednou per slovník
        │     ├── Notes (poznámka, sdílená napříč skupinami)
        │     ├── Tags (přes DictionaryEntryTag, sdílené napříč skupinami)
        │     ├── GroupIds[] (ve kterých skupinách je, bigint array + GIN index)
        │     └── IsActive (soft delete)
        └── WordGroup = pojmenovaná kolekce (M:N přes GroupIds array)
              ├── IsPublic (veřejná skupina)
              └── SourceGroupId (fork zdroj)

Word, WordPair = globální, deduplikované, bez vlastníka
UserWordProgress = per uživatel per WordPair (ne per skupina)
TrainingSession → TrainingResult = historie tréninku
```

### Klíčové principy

- **Deduplikace slov:** Word("apple", EN) existuje jednou v DB. WordPair("jablko"↔"apple") existuje jednou. Sdílené napříč uživateli.
- **Slovíčko ve slovníku jednou:** DictionaryEntry per (DictionaryId, WordPairId) — unique constraint. Poznámky a tagy jsou na této úrovni.
- **Skupiny = M:N organizace:** Slovíčko může být ve více skupinách. Členství = `GroupIds bigint[]` array na DictionaryEntry (žádná junction tabulka).
- **Slovíčko patří do slovníku, ne do skupiny:** Smazání skupiny neodstraní slovíčka ze slovníku — jen odebere groupId z GroupIds. Slovíčka existují nezávisle na skupinách.
- **Fork skupiny:** Pro každé slovíčko: existuje v mém slovníku → jen přiřadit do skupiny (moje poznámky/tagy zůstanou). Neexistuje → vytvořit DictionaryEntry s kopií.
- **Progress globální:** UserWordProgress navázán na WordPairId. Stejný překlad ve více skupinách = jeden progress.
- **Find-or-create:** Při přidání slovíčka: find-or-create Word → WordPair → DictionaryEntry → přiřadit do skupiny.
- **Validace duplicity slovníku:** Nelze vytvořit dva slovníky se stejným jazykovým párem (server + klient).

### DB schéma

Kompletní specifikace v `lexitrek-db-design.md`.

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
| `LexiTrek.Application` | Use cases, business logika, rozhraní služeb (IAuthService, IDictionaryService, IGroupService…) |
| `LexiTrek.Domain` | Doménové entity (Dictionary, WordGroup, DictionaryEntry, Word, WordPair, Tag…), žádné závislosti |
| `LexiTrek.Infrastructure` | EF Core DbContext, ASP.NET Identity, implementace služeb |
| `LexiTrek.Shared` | Sdílené DTO a enumy (používá Web i Application) |
| `LexiTrek.Tests` | xUnit testy pro Domain a Application |

### API endpointy

| Endpoint | Popis |
|----------|-------|
| `GET/POST/DELETE api/dictionaries` | CRUD slovníků (jazykových párů) |
| `GET/POST/PUT/DELETE api/groups` | CRUD skupin |
| `GET api/groups/public` | Veřejné skupiny (bez vlastních) |
| `POST api/groups/{id}/fork` | Fork veřejné skupiny |
| `POST/DELETE api/groups/{id}/entries/{entryId}` | Přiřazení/odebrání slovíčka ze skupiny |
| `GET api/dictionaries/{id}/entries` | Všechna slovíčka ve slovníku |
| `GET api/groups/{id}/entries` | Slovíčka filtrovaná skupinou |
| `POST api/dictionaries/{id}/entries` | Přidání slovíčka (+ volitelné ?groupId) |
| `PUT/DELETE api/entries/{id}` | Úprava/smazání slovíčka |
| `POST/DELETE api/entries/{id}/tags` | Přiřazení/odebrání tagů |
| `GET/POST/PUT/DELETE api/tags` | CRUD tagů |
| `GET api/training/words` | Slovíčka k tréninku |
| `POST api/training/sessions` | Zahájení session |
| `PUT api/training/sessions/{id}/complete` | Dokončení session |
| `GET api/languages` | Seznam jazyků |
| `POST api/auth/register\|login\|refresh` | Autentizace |

### UI stránky

| Route | Stránka | Popis |
|-------|---------|-------|
| `/` | Home | Onboarding (bez slovníku) / Dashboard (se slovníkem) / Landing (nepřihlášený) |
| `/groups` | Groups | Moje skupiny + virtuální "Všechna slovíčka" (filtrované podle slovníku v AppBar) |
| `/groups/create` | DictionaryCreate | Vytvoření skupiny (název, popis, veřejná/soukromá). Podporuje ?entryIds pro hromadné přiřazení. |
| `/groups/{id}` | DictionaryDetail | Detail skupiny — slovíčka, přidání, editace, trénink, checkboxy, hromadné přiřazení do skupiny |
| `/groups/public` | PublicDictionaries | Veřejné skupiny (bez vlastních), klikací karty → detail s tlačítkem Převzít. Podporuje ?search. |
| `/dictionaries/{id}/entries` | AllEntries | Virtuální skupina "Všechna slovíčka" — přidání, editace, smazání, checkboxy, hromadné přiřazení |
| `/settings` | Settings | Správa slovníků (vytvoření jazykového páru, validace duplicity) |
| `/tags` | Tags | Správa tagů |
| `/training/setup` | TrainingSetup | Výběr skupiny/tagu a počtu slov |
| `/training/{id}` | Training | Kartičkový trénink (flip, Red/Orange/Green) |
| `/training/{id}/results` | TrainingResults | Výsledky tréninku |

### UX vzory

- **Onboarding:** Nový uživatel bez slovníku → inline výběr jazyků na homepage, bez levého menu
- **AppBar:** Logo (klikací → homepage), slovník selector, settings menu (dropdown), profil menu
- **Přepnutí slovníku:** Vždy naviguje na homepage a refreshne data
- **Karty skupin:** Klikací (group-card class pro hover), ikona veřejná/soukromá s tooltipem, bez popisu/slovníku
- **Detail skupiny:**
  - Kompaktní hlavička: název + info ikona (dropdown s jazykem, autorem, popisem) + switch veřejná/soukromá
  - Sticky akční lišta: přidat slovíčko, trénink, přidat do skupiny (po checkboxu), vyhledávání (flex-grow)
  - Checkboxy pro hromadný výběr s "Přidat do skupiny" menu (existující + nová)
  - Inline formulář pro přidání slovíčka
  - Cizí skupina: "Převzít vše" (fork celé skupiny) + checkboxy s "Převzít vybraná" do vlastní skupiny. Bez editace/smazání slovíček a skupiny, bez switch veřejná/soukromá. Autor chip s proklikem na veřejné skupiny.
- **Virtuální skupina "Všechna slovíčka":** Vždy první v seznamu skupin (i bez vlastních skupin). Počet = unikátní entries ze serveru (DictionaryListDto.EntryCount).
- **Settings:** Dropdown menu v AppBar. Atraktivní stránka — onboarding styl (bez slovníků) / kompaktní formulář + grid (se slovníky). Filtr cílového jazyka (nesmí být stejný jako zdrojový).
- **Formuláře:** Atraktivní karty se zaoblenými rohy, ikony v inputech, switch veřejná/soukromá s vysvětlením
- **Konzistence:** mt-4 odsazení na všech stránkách, h5 nadpisy, zaoblené karty (border-radius: 12px)

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

**MudTable s checkboxy:**
- `MultiSelection="true"` + `@bind-SelectedItems` pro hromadný výběr
- `Filter="FilterFunc"` pro klientské vyhledávání

**DictState refresh:**
- Po přidání/smazání slovíčka vždy volat `await DictState.LoadAsync()` pro aktualizaci EntryCount

## Autentizace

- **ASP.NET Identity** s vlastním `AppUser` entity
- **JWT Bearer** — access token (15 min), refresh token (7 dní)
- Konfigurace: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` v appsettings
- Refresh tokeny uloženy jako shadow properties na `AppUser` (EF Core)
- **Klient (WASM):** `TokenStorageService` ukládá tokeny do `localStorage`, `JwtAuthStateProvider` parsuje JWT claims, `AuthHeaderHandler` přidává Bearer header
- **MainLayout:** `OnParametersSetAsync` načte slovníky po přihlášení (ne `OnInitializedAsync` — ten běží jen jednou)

## Git

- **NIKDY nepřidávat `Co-Authored-By: Claude` ani žádnou zmínku o Claude jako autora/spoluautora do commit messages.** Autor commitů je vždy uživatel.
- Default branch: `master`

## Klíčové konvence

- **.NET 10**, C# latest, nullable enabled, implicit usings
- **PostgreSQL 17** s `pg_trgm` rozšířením pro fulltext
- **ID typy:** `long` (bigint) pro doménové entity, `string` pro AppUser (ASP.NET Identity)
- **Composite PK:** `DictionaryEntry(Id, DictionaryId)`, `UserWordProgress(UserId, WordPairId)` — připraveno pro HASH partitioning
- **Soft delete:** `IsActive` flag na DictionaryEntry (ne fyzické mazání)
- **Array sloupec:** `GroupIds bigint[]` s GIN indexem místo junction tabulky (optimalizace pro škálování)
- **Sticky lišta:** Akční tlačítka + vyhledávání na detailu skupiny — `position: sticky; top: 64px`
