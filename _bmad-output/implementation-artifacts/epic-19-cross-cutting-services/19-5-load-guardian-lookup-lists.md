# Story 19-5: Load guardian lookup lists

| Field | Value |
| --- | --- |
| Story key | `19-5-load-guardian-lookup-lists` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-05 — قوائم العائل |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.5 scenario) |
| Route | none — the lists feed the guardian (provider/parent) sections of the family forms |
| Endpoint | `GET /api/LookupManagement/marital-statuses` (new) + the live `relations` / `reasons-of-relation` reads |
| Depends on | 19-4 pattern (LookupServiceBase chain); consumers are the EP-05 family forms |
| Roles | All roles (authenticated) |

## Status

done

## Story

As a signed-in user, I want to be able to load guardian lookup lists قوائم العائل, so that I
can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a signed-in user with an active session, when a host screen loads, then the guardian
   reference lists that screen needs are served — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/...` and the response is rendered on the screen without a page
   reload.
3. Given the guardian record's marital status is requested, when the read is served, then the
   list comes from a dedicated marital-status catalogue (`LookupEntity`, `NameAr`/`NameEn`,
   `IsActive` respected).
4. Given a catalogue is empty, then the consuming control renders its empty option — not an
   error (8-11 AC 6 precedent).
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §24.U.5 passes for the three guardian lists (marital
status, relation to orphans, job categories → jobs are 19-9's endpoint, cross-referenced);
scoping notes recorded (catalogue reads are global reference data per the 16-8/19-4 ruling).

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Relation lists | `LookupManagementController` — `GET relations` + `GET reasons-of-relation` (entities `Relation`, `ReasonOfRel`, added by epic 7) | Live — serves "relation to orphans" |
| Pattern | `LookupServiceBase` 7-file chain — the exact stack 8-11 shipped twice (education-levels, health-statuses) | Copy it verbatim for marital status |
| Social status | `GET social-statuses` (entity `SocialStatus`, epic 7 — orphan's social status) | Live — **not** the guardian's marital status; do not conflate |
| Job categories | **No `Job` entity, no jobs endpoint exists** | 19-9 builds it — this story only cross-references |
| Frontend | `lookup-management.service.ts` method-per-catalogue idiom; `app-drop-down` + `LookupService` cache | Live |

## Verified gaps this story must fix

1. **No marital-status catalogue exists.** The Domain lookup set (Department, MissionType…,
   EducationLevel, HealthStatus, HouseOwnership, HouseStatus, IncomeType, SocialStatus,
   Relation, ReasonOfRel, HousingType, City, LivingCondition, …) has **no MaritalStatus**.
   First task: `grep` the Domain for an existing entity serving it (reuse if found); otherwise
   create `MaritalStatus : LookupEntity` + the full 7-file chain +
   `GET /api/LookupManagement/marital-statuses`.
2. **Seeding.** A new catalogue renders empty until seeded — ship an existence-guarded,
   Arabic-first seed (أعزب، متزوج، مطلق، أرمل — mirror with `NameEn`) via the established
   seeder idiom (`IIROSASeedDataInitializer`).

## Tasks / Subtasks

- [x] **Task 1 — MaritalStatus lookup stack** (AC 2, 3)
  - [x] Verified none exists (only the periodic-report filter string); chain landed; then: entity (`LookupEntity`, LOOKUP_SCHEMA) +
        configuration, `IMaritalStatusRepository` + typed repo, `IMaritalStatusService` +
        `LookupServiceBase` service, DTO trio, AutoMapper profile, DI registration,
        `GET marital-statuses` action — byte-for-byte the 8-11 shape (`{ id, nameAr, nameEn }`
        rows, `IsActive` respected, raw envelope)
  - [x] No new caching (none of the lookups cache — platform-consistent)
- [x] **Task 2 — Seed** (AC 4)
  - [x] Existence-guarded Arabic-first rows (marital statuses above), `IsActive = true`,
        wired through the seeder initializer
- [x] **Task 3 — Frontend service + consumer surface** (AC 1, 4)
  - [x] `lookup-management.service.ts`: `getMaritalStatuses()` in the guardian section of the
        file (fix the mis-filed `getRefuseReasons` header only if 19-6 hasn't — coordinate)
  - [x] Guardian-form wiring itself belongs to EP-05 (no guardian form section consumes it
        today — 8-11 recorded the same deferral for its lists); leave the endpoint live and
        record the deferral
- [x] **Task 4 — Verification** (AC 5)
  - [x] Builds green; live smoke: `marital-statuses` → 200 with seeded rows (or `[]` +
        empty-option note pre-seed-restart), unauthenticated → 401. Tests excluded per the
        standing decision

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>`** on this controller (2026-08-19 standing decision); lookups are
  `LookupEntity` (int) in `MappingDefaults.LOOKUP_SCHEMA`; no manual `DbSet`; `NameAr ?? NameEn`
  labelling; Arabic primary.
- New catalogue = the 7-file chain, no shortcuts (8-11's bar).
- Catalogue reads are global reference data — no charity scoping on the rows (16-8/19-4
  ruling); the guardian record that references them carries the tenancy.
- EF migration: a new lookup table needs one — **check the last applied migration chain first**
  (parallel-session bundling hazard: recent migrations swept unrelated model deltas). The new
  migration must contain ONLY the MaritalStatus table; verify with `dotnet ef migrations list`
  against `__EFMigrationsHistory` before adding, and hand-prune any spurious
  `DropTable("ApplicationUser")` the stale snapshot emits (epic-15 recorded hazard).

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Job categories (المهنة) | 19-9 |
| Generic sweep (page caps, export gating) | 19-4 |
| Guardian form consumption | EP-05 refinement (recorded deferral, 8-11 precedent) |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.5] scenario + §24.2 UC-SYS-05 row (marital status, relation to orphans, job categories)
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-05 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-8-orphan-register-and-coding/8-11-load-orphan-reference-data.md] the 7-file chain pattern to copy + the wiring deferral precedent
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/] the lookup entity set (no MaritalStatus today)

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- Migration isolation proven: `20260825095340_Epic19_MaritalStatusLookup` contains exactly one
  `CreateTable("MaritalStatus", "Lookup")` + matching `DropTable` — zero spurious operations.
- **EF-tooling note (reusable):** `dotnet ef` resolves the target assembly through the STARTUP
  project's bin — the user's live API locks `Api/bin/Debug`, so Debug-config EF runs diff a stale
  model and silently emit EMPTY migrations. Working route: build + run EF with
  `--configuration Release` (separate output, unlocked). First --no-build attempt produced an
  empty `Up()` twice before this was diagnosed.
- Live battery (61970): `GET marital-statuses` → 200 `[{أعزب,Single},{متزوج,Married},{مطلق,Divorced},{أرمل,Widowed}]`
  all active; Charity → 200; unauthenticated → 401; seeder Step 9 fired on startup.
- Frontend `tsc --noEmit` clean.

### Completion Notes List

- 7-file chain copied byte-for-byte from the 8-11/EducationLevel pattern: entity +
  configuration (LOOKUP_SCHEMA), `IMaritalStatusRepository` + repo, `IMaritalStatusService` +
  `LookupServiceBase` service, DTO trio, `LookupProfile` maps, DI ×2, controller DI +
  `GET marital-statuses` (active-only, `DropdownPageSize`, raw envelope).
- Seed via new `GuardianLookupSeedData` (existence-guarded, mirrors `RefuseReasonSeedData`
  idiom) wired as seeder Step 9 — Job (19-9) will join this seeder's `SeedAllAsync`.
- `getMaritalStatuses()` added under a new UC-SYS-05 section in `lookup-management.service.ts`;
  the mis-filed `getRefuseReasons` header left for 19-6 (owning story, per the task note).
- Guardian-form consumption deferred to EP-05 (8-11 precedent) — endpoint live, no consumer
  wired; empty catalogue renders empty option (8-11 AC 6 precedent).

### File List

- Backend/src/IIROSA.Domain/Entities/Lookups/MaritalStatus.cs (new)
- Backend/src/IIROSA.Domain/Configurations/MaritalStatusConfiguration.cs (new)
- Backend/src/IIROSA.Domain/Interfaces/ILookupRepository.cs (IMaritalStatusRepository)
- Backend/src/IIROSA.Infrastructure/Data/Repository/LookupRepository.cs (MaritalStatusRepository)
- Backend/src/IIROSA.Application/Interfaces/ILookupService.cs (IMaritalStatusService)
- Backend/src/IIROSA.Application/Services/LookupManagementService.cs (MaritalStatusService)
- Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs (MaritalStatusDto trio)
- Backend/src/IIROSA.Application/Profiles/LookupProfile.cs (MaritalStatus maps)
- Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs (service + repo DI)
- Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs (DI + GET marital-statuses)
- Backend/src/IIROSA.Infrastructure/Data/SeedData/GuardianLookupSeedData.cs (new)
- Backend/src/IIROSA.Infrastructure/Data/SeedData/IIROSASeedDataInitializer.cs (Step 9)
- Backend/src/IIROSA.Infrastructure/Data/Migrations/20260825095340_Epic19_MaritalStatusLookup.cs (+Designer, snapshot) — applied
- Frontend/src/app/modules/lookup-management/services/lookup-management.service.ts (getMaritalStatuses)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-05 and module spec §24.U.5; resolved to marital-status catalogue (new) + live relation lists + jobs cross-ref (19-9); guardian-form wiring deferred to EP-05 per the 8-11 precedent. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): full 7-file MaritalStatus chain + isolated migration (single CreateTable, applied) + existence-guarded seed + getMaritalStatuses; live battery green on 61970; Release-config EF route recorded after Debug-bin lock produced empty migrations. |
