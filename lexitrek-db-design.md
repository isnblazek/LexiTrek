# LexiTrek — Databázové schéma

## Principy návrhu

- **Slova jsou globální** — `Word` a `WordPair` jsou sdílené napříč všemi uživateli, bez vlastníka. Slovo "apple" v EN existuje v DB jednou, ať si ho přidá 1000 uživatelů.
- **Slovník = jazykový pár** — `Dictionary` je per uživatel, definuje pouze zdrojový a cílový jazyk (CZ↔EN). Bez názvu, popisu ani viditelnosti.
- **Slovíčko existuje ve slovníku jednou** — `DictionaryEntry` patří do `Dictionary`, každý překlad (WordPair) se v daném slovníku vyskytuje maximálně jednou. Poznámky a tagy jsou na úrovni slovníku a sdílejí se napříč skupinami.
- **Skupiny = pojmenovaná M:N organizace** — `WordGroup` je pojmenovaná kolekce slovíček. Jedno slovíčko může být ve více skupinách současně — členství je uloženo jako `GroupIds bigint[]` array sloupec na `DictionaryEntry` s GIN indexem (žádná junction tabulka). Skupiny mohou být veřejné.
- **Hard copy fork skupiny** — při převzetí veřejné skupiny se pro každé slovíčko: (a) pokud už existuje v mém slovníku → jen se přiřadí do skupiny (moje poznámky/tagy zůstanou), (b) pokud neexistuje → vytvoří se nový DictionaryEntry s kopií poznámek/tagů.
- **Progress je globální per uživatel** — `UserWordProgress` je navázán na `WordPairId`, ne na slovník ani skupinu. Stejný překlad ve více skupinách sdílí jeden progress záznam.
- **Trénink** — `TrainingSession` a `TrainingResult` zaznamenávají historii, `UserWordProgress` řídí spaced repetition (SM-2). Trénink je per skupina nebo per tag.

---

## Schéma

```sql
-- ══════════════════════════════════════════
-- Jazyky (seed 12 jazyků: cs, en, de, fr, es, it, pl, sk, ru, pt, nl, uk)
-- ══════════════════════════════════════════
CREATE TABLE "Languages" (
    "Id"    serial PRIMARY KEY,
    "Code"  varchar(10) NOT NULL,
    "Name"  varchar(100) NOT NULL
);

-- ══════════════════════════════════════════
-- Globální deduplikovaná slovíčka (bez vlastníka)
-- ══════════════════════════════════════════
CREATE TABLE "Words" (
    "Id"          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Text"        varchar(500) NOT NULL,
    "LanguageId"  int NOT NULL REFERENCES "Languages"("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_Words_Text_LanguageId" ON "Words"("Text", "LanguageId");

-- ══════════════════════════════════════════
-- Globální deduplikované překladové páry (bez vlastníka)
-- ══════════════════════════════════════════
CREATE TABLE "WordPairs" (
    "Id"            bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "SourceWordId"  bigint NOT NULL REFERENCES "Words"("Id") ON DELETE RESTRICT,
    "TargetWordId"  bigint NOT NULL REFERENCES "Words"("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_WordPairs_SourceWordId_TargetWordId" ON "WordPairs"("SourceWordId", "TargetWordId");

-- ══════════════════════════════════════════
-- Slovníky (jazykový pár per uživatel, bez názvu)
-- ══════════════════════════════════════════
CREATE TABLE "Dictionaries" (
    "Id"            bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"        text NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "SourceLangId"  int NOT NULL REFERENCES "Languages"("Id") ON DELETE RESTRICT,
    "TargetLangId"  int NOT NULL REFERENCES "Languages"("Id") ON DELETE RESTRICT,
    "CreatedAt"     timestamptz NOT NULL
);
CREATE INDEX "IX_Dictionaries_UserId" ON "Dictionaries"("UserId");

-- ══════════════════════════════════════════
-- Slovíčka ve slovníku (jedno slovíčko = jeden záznam per slovník)
-- Composite PK (Id, DictionaryId) — připraveno pro HASH partitioning
-- Poznámky a tagy jsou na této úrovni — sdílené napříč skupinami
-- ══════════════════════════════════════════
CREATE TABLE "DictionaryEntries" (
    "Id"            bigint GENERATED ALWAYS AS IDENTITY,
    "DictionaryId"  bigint NOT NULL REFERENCES "Dictionaries"("Id") ON DELETE CASCADE,
    "WordPairId"    bigint NOT NULL REFERENCES "WordPairs"("Id") ON DELETE RESTRICT,
    "IsActive"      boolean NOT NULL DEFAULT true,
    "Notes"         varchar(1000),
    "GroupIds"      bigint[] NOT NULL DEFAULT '{}',
    PRIMARY KEY ("Id", "DictionaryId")
);
CREATE UNIQUE INDEX "IX_DictionaryEntries_DictionaryId_WordPairId"
    ON "DictionaryEntries"("DictionaryId", "WordPairId");
CREATE INDEX "IX_DictionaryEntries_WordPairId"
    ON "DictionaryEntries"("WordPairId");
CREATE INDEX "IX_DictionaryEntries_GroupIds"
    ON "DictionaryEntries" USING gin ("GroupIds");

-- ══════════════════════════════════════════
-- Skupiny (pojmenovaná kolekce slovíček, M:N)
-- ══════════════════════════════════════════
CREATE TABLE "WordGroups" (
    "Id"              bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name"            varchar(200) NOT NULL,
    "Description"     varchar(1000),
    "OwnerId"         text NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "DictionaryId"    bigint NOT NULL REFERENCES "Dictionaries"("Id") ON DELETE RESTRICT,
    "IsPublic"        boolean NOT NULL DEFAULT false,
    "SourceGroupId"   bigint REFERENCES "WordGroups"("Id") ON DELETE SET NULL,
    "CreatedAt"       timestamptz NOT NULL,
    "UpdatedAt"       timestamptz NOT NULL
);
CREATE INDEX "IX_WordGroups_OwnerId" ON "WordGroups"("OwnerId");
CREATE INDEX "IX_WordGroups_DictionaryId" ON "WordGroups"("DictionaryId");
CREATE INDEX "IX_WordGroups_IsPublic" ON "WordGroups"("IsPublic");

-- ══════════════════════════════════════════
-- Tagy (uživatelské, unikátní per (OwnerId, Name))
-- ══════════════════════════════════════════
CREATE TABLE "Tags" (
    "Id"        bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name"      varchar(100) NOT NULL,
    "OwnerId"   text NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "CreatedAt" timestamptz NOT NULL
);
CREATE UNIQUE INDEX "IX_Tags_OwnerId_Name" ON "Tags"("OwnerId", "Name");

-- ══════════════════════════════════════════
-- Tagy na slovíčkách (na úrovni slovníku, sdílené napříč skupinami)
-- FK na DictionaryEntry vyžaduje oba sloupce composite PK
-- ══════════════════════════════════════════
CREATE TABLE "DictionaryEntryTags" (
    "DictionaryEntryId"  bigint NOT NULL,
    "DictionaryId"       bigint NOT NULL,
    "TagId"              bigint NOT NULL REFERENCES "Tags"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("DictionaryEntryId", "DictionaryId", "TagId"),
    FOREIGN KEY ("DictionaryEntryId", "DictionaryId")
        REFERENCES "DictionaryEntries"("Id", "DictionaryId") ON DELETE CASCADE
);

-- ══════════════════════════════════════════
-- Tréninkové sessions (per skupina nebo per tag)
-- ══════════════════════════════════════════
CREATE TABLE "TrainingSessions" (
    "Id"              bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UserId"          text NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "Mode"            int NOT NULL,          -- 0 = Group, 1 = Tag
    "GroupId"         bigint REFERENCES "WordGroups"("Id") ON DELETE SET NULL,
    "TagId"           bigint REFERENCES "Tags"("Id") ON DELETE SET NULL,
    "StartedAt"       timestamptz NOT NULL,
    "CompletedAt"     timestamptz,
    "IsCompleted"     boolean NOT NULL DEFAULT false,
    "ClientSessionId" uuid NOT NULL
);
CREATE INDEX "IX_TrainingSessions_UserId" ON "TrainingSessions"("UserId");
CREATE INDEX "IX_TrainingSessions_ClientSessionId" ON "TrainingSessions"("ClientSessionId");

-- ══════════════════════════════════════════
-- Výsledky tréninku (per slovíčko per session)
-- ══════════════════════════════════════════
CREATE TABLE "TrainingResults" (
    "Id"          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "SessionId"   bigint NOT NULL REFERENCES "TrainingSessions"("Id") ON DELETE CASCADE,
    "WordPairId"  bigint NOT NULL REFERENCES "WordPairs"("Id") ON DELETE RESTRICT,
    "Result"      int NOT NULL,              -- 0 = Red, 1 = Orange, 2 = Green
    "AnsweredAt"  timestamptz NOT NULL
);
CREATE INDEX "IX_TrainingResults_SessionId" ON "TrainingResults"("SessionId");

-- ══════════════════════════════════════════
-- Spaced repetition progress (per uživatel, per překlad)
-- Composite PK (UserId, WordPairId) — připraveno pro HASH partitioning
-- ══════════════════════════════════════════
CREATE TABLE "UserWordProgresses" (
    "UserId"        text NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "WordPairId"    bigint NOT NULL REFERENCES "WordPairs"("Id") ON DELETE RESTRICT,
    "EaseFactor"    double precision NOT NULL DEFAULT 2.5,
    "IntervalDays"  int NOT NULL DEFAULT 1,
    "Repetitions"   int NOT NULL DEFAULT 0,
    "NextReview"    date NOT NULL,
    "LastReviewedAt" timestamptz,
    PRIMARY KEY ("UserId", "WordPairId")
);
CREATE INDEX "IX_UserWordProgresses_UserId" ON "UserWordProgresses"("UserId");
CREATE INDEX "IX_UserWordProgresses_UserId_NextReview"
    ON "UserWordProgresses"("UserId", "NextReview");
```

---

## Vztahy mezi entitami

```
AppUser (ASP.NET Identity)
  ├── 1:N  Dictionaries       (UserId)
  ├── 1:N  WordGroups          (OwnerId)
  ├── 1:N  Tags                (OwnerId)
  ├── 1:N  TrainingSessions    (UserId)
  └── 1:N  UserWordProgresses  (UserId)

Language
  └── referenced by: Word, Dictionary (source/target)

Word (globální, deduplikované)
  └── referenced by: WordPair (source/target)

WordPair (globální, deduplikované)
  ├── referenced by: DictionaryEntry
  ├── referenced by: TrainingResult
  └── referenced by: UserWordProgress

Dictionary (jazykový pár per uživatel)
  ├── 1:N  DictionaryEntries   (slovíčka ve slovníku)
  └── 1:N  WordGroups           (skupiny pod slovníkem)

DictionaryEntry (slovíčko ve slovníku — jednou, s poznámkami a tagy)
  ├── N:M  Tags                (přes DictionaryEntryTag)
  └── N:M  WordGroups          (přes GroupIds bigint[] array, GIN index)

WordGroup (pojmenovaná kolekce)
  ├── 0:1  SourceGroup         (self-FK, fork zdroj)
  └── referenced by: TrainingSession, DictionaryEntry.GroupIds
```

---

## Fork skupiny

```
Uživatel B forkne veřejnou skupinu "Jídlo" od uživatele A:

1. Najít/vytvořit slovník CZ↔EN pro uživatele B
2. Vytvořit WordGroup "Jídlo" pro uživatele B (SourceGroupId = originál)
3. Pro každé slovíčko ve zdrojové skupině:
   a) Existuje DictionaryEntry v mém slovníku? → jen přiřadit do skupiny
      (moje poznámky a tagy zůstanou beze změny)
   b) Neexistuje? → vytvořit DictionaryEntry (zkopírovat poznámky/tagy)
4. Přiřadit všechna slovíčka do nové skupiny (přidat fork.Id do GroupIds array)
```

---

## Find-or-create logika (přidání slovíčka)

Při přidání slovíčka do slovníku (volitelně i do skupiny):

1. **Find-or-create Word** pro source text + source language
2. **Find-or-create Word** pro target text + target language
3. **Find-or-create WordPair** pro (source_word_id, target_word_id)
4. **Find-or-create DictionaryEntry** (dictionary_id, word_pair_id, notes)
   - Pokud existuje a je aktivní → jen přiřadit do skupiny
   - Pokud existuje ale je soft-deleted → reaktivovat
   - Pokud neexistuje → vytvořit
5. **Přiřadit do skupiny** (pokud je zadána) — přidat groupId do GroupIds array

---

## HASH partitioning (volitelné, pro produkci)

Aktuální implementace používá standardní tabulky. Pro produkční nasazení s miliony uživatelů lze přidat HASH partitioning na `DictionaryEntries` a `UserWordProgresses`:

```sql
CREATE TABLE "DictionaryEntries" (
    ...
) PARTITION BY HASH ("DictionaryId");
-- 64 particí: DictionaryEntries_0 ... DictionaryEntries_63
```

Partition key musí být součástí PK — proto je PK `(Id, DictionaryId)`.

**Odhad velikosti:**

| Scale | DictionaryEntry řádků | Velikost (~30 B/řádek) |
|---|---|---|
| 10k uživatelů × 500 slov | 5M | ~150 MB |
| 100k uživatelů × 500 slov | 50M | ~1.5 GB |
| 1M uživatelů × 500 slov | 500M | ~15 GB |

---

## Spaced repetition (SM-2)

Trénink probíhá přes `TrainingSession` → `TrainingResult`:

1. **Výběr slov:** Přes `GroupIds @> ARRAY[groupId]` (při mode=Group) nebo DictionaryEntryTags (při mode=Tag) → DictionaryEntry → WordPair. Priorita 1 = nová (bez UserWordProgress), priorita 2 = k opakování (NextReview <= today).
2. **Hodnocení:** Red (0) = nevím, Orange (1) = částečně, Green (2) = vím
3. **Aktualizace progressu:** SM-2 algoritmus upraví `EaseFactor`, `IntervalDays`, `NextReview`

Progress je navázán na `WordPairId` — stejný překlad sdílí jeden progress záznam napříč všemi skupinami a slovníky.
