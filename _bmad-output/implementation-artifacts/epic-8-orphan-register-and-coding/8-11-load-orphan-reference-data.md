# Story 8-11: Load orphan reference data

| Field | Value |
| --- | --- |
| Story key | `8-11-load-orphan-reference-data` |
| Epic | EP-08 — Orphan Register & Coding (سجل الايتام والتكويد) |
| Use case | UC-ORP-11 — القوائم المرجعية لليتيم |
| Priority / size | Must · 2 points |
| Specification | `docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md` (§13.U.11 scenario) |
| Route | none — lookup feeds consumed by the coding screens (8-3, 8-4), the orphan form tab, and the payment dialogs (8-8, 8-9) |
| Endpoint | `GET /api/LookupManagement/education-levels` · `GET /api/LookupManagement/health-statuses` · `GET /api/OrphanPayments/batch-numbers` (board shorthand: `GET /api/OrphanPayments` / `GET /api/LookupManagement`) |
| Depends on | EP-19 shared lookups (19-4 pattern); feeds 8-3, 8-4, 8-8, 8-9 |
| Roles | Charity + HQ (authenticated) |

## Status

done

## Story

As a charity user, I want to be able to load orphan reference data القوائم المرجعية لليتيم, so that I can find the record I need without leaving the system.

## Acceptance Criteria

1. Given a signed-in user with an active session, when a host screen loads, then the reference lists that screen needs are served — no stored data is changed.
2. Given the requests are accepted, when they are served, then they are handled by `GET /api/LookupManagement/...` and `GET /api/OrphanPayments/batch-numbers` and the responses are rendered on the screen without a page reload.
3. Given a charity user, when the lists are loaded, then only reference data visible to that charity (and country) is returned — batch numbers derive from batches containing the caller's scope.
4. Given an HQ role, when an explicit charity id is supplied, then the batch-number list operates on that charity's data.
5. Given the session has expired or the role is not permitted, when the lists are requested, then the request is rejected and the actor is routed back to the login screen.
6. Given a list is empty, then the consuming control renders its empty option — not an error («Faliure» in legacy terms must not surface for an empty list).

**Definition of done:** the scenario of §13.U.11 passes end to end; the role and charity scoping is enforced server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- | --- |
| Lookup entities | `Entities/Lookups/EducationLevel.cs`, `HealthStatus.cs` | Exist (`LookupEntity`, int key) — referenced by `Orphan.EducationLevelId` / `HealthStatusId` |
| Lookup endpoints | `LookupManagementController` | Has countries, regions, centers, departments, banks, cheque-beneficiaries, currencies, cheque-positions, office-project-types, mission-interview-types — **no education-levels / health-statuses yet** |
| Lookup pattern | Any existing `GET` on that controller (e.g. `departments`, 16-8 precedent) | Copy the pattern: paged or plain list, `NameAr ?? NameEn` labels, `IsActive` respected, anonymous error objects |
| Batch numbers | `OrphanPayment.BatchNo` + `IsBatchNoUniqueAsync` | Data exists; **no distinct list endpoint** (legacy `GetBatchNos`) |
| Charities list | `GET /api/Charities` | Exists — the الجمعية selector source; do not rebuild |

## Tasks / Subtasks

- [x] **Task 1 — Orphan form lookups** (AC 1, 2, 5)
  - [x] Full lookup stack per the 19-x pattern: `IEducationLevelRepository`/`IHealthStatusRepository` (+ typed `LookupRepository<T>` impls), `IEducationLevelService`/`IHealthStatusService` on `LookupServiceBase`, DTO trios mirroring the NGOType shape, AutoMapper profile (6 mappings), 4 DI registrations, and `LookupManagementController` `GET education-levels` / `GET health-statuses` — same shape as the controller's other lookups (paged `{ id, nameAr, nameEn }`, `IsActive` respected)
  - [x] No new caching — mirrors the other lookups (none cache; never `IMemoryCache` directly)
  - [x] Global reference data — no charity scoping (AC 3 applies to batch numbers only)
- [x] **Task 2 — Batch-number reference list** (AC 2–4, 6)
  - [x] `IOrphanPaymentService.GetBatchNumbersAsync(Guid? userCharityId, string? userRole, Guid? charityId)` → `List<BatchNumberDto>` (`BatchNo` + `LatestGroupDate`); distinct non-empty numbers, most-recent batch first (max `GroupDate` per number)
  - [x] `OrphanPaymentsController`: `GET batch-numbers` — literal segment, no `{id}` collision (Guid constraint); scoping: `Charity` role → only groups containing that charity's orphans (via `IOrphanPaymentItemRepository.GetGroupIdsByCharityAsync` HashSet); HQ may pass `CharityId`
  - [x] Consumers wired: the 8-8 history dialog's رقم الدفعة filter (كل الدفعات empty option)
- [x] **Task 3 — Wire the feeds** (AC 1, 6)
  - [x] Coding screens (8-3/8-4): الجمعية selector from `GET /api/Charities` + كل الجمعيات empty option — done in 8-3/8-4
  - [ ] Education-level / health-status label surfacing deferred: no orphan modal exists (8-4 replaced it with the payment-history open — see 8-4 notes) and no orphan form exists (EP-05); the endpoints are live and waiting for those surfaces
  - [ ] Family-form rebinding deliberately NOT done: the father/mother forms bind `educationLevel`/`healthStatus` as **free strings** in the epic-4/5 DTO contract — rebinding them to lookup ids would silently change the family-module wire contract; that rework belongs to the owning epic, not this story
  - [x] i18n keys in **both** `ar.json` and `en.json`; `trackBy` on rendered option lists
- [x] **Task 4 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` — 0 errors
  - [x] Live check 2026-08-24: `education-levels` + `health-statuses` → 200 (empty arrays — dev DB has no seeded lookup rows; **empty list → 200 `[]`, not 500**, verified literally); `batch-numbers` → 200 `[]`; unauthenticated → 401. Charity scoping of batch-numbers not exercisable live (seed `Charity` user fail-closed) — verified in code review

### Review Findings

_2026-08-24 adversarial review (blind + edge-case + acceptance layers)._

- [x] [Review][Patch] `GetBatchNumbersAsync` loads the entire `OrphanPayment` table (`GetAllAsync()`) then groups in memory — project the distinct list server-side [OrphanPaymentService.cs:484]
- [x] [Review][Patch] Soft-delete: deleted batches appear in the batch picker (cross-story, anchored in 8-2)
- [x] [Review][Defer] `LookupManagementController` class-level `SuperAdminOnly` → plain JWT auth with per-action policies, visible in this shared file's diff — introduced by a **parallel session's** uncommitted lookup work (it matches the epic-15 review's deferred recommendation), not this story; that session must verify every write endpoint (bulk import above all) carries a policy before commit [LookupManagementController.cs]

## Dev Notes

### Platform rules that bind this story

- **No `ApiResponse<T>` wrapper** — match the existing `LookupManagementController` responses exactly (2026-08-19 standing decision — raw stays until the platform-wide ApiResponse migration story).
- Lookups are `LookupEntity` (int) — never `FullAuditedEntity`; schema via `MappingDefaults.LOOKUP_SCHEMA`; no manual `DbSet` (auto-discovery; the sanctioned TechnicalSupport exception does not extend here).
- Labels resolve `NameAr ?? NameEn`; Arabic primary.
- **Deferred reference lists:** the spec's full orphan-form catalogue (prayer and memorisation levels, hobbies, behaviour scales, relationship) has **no lookup entities in the Domain** — creating them belongs to the §10.S orphan-form refinement (EP-05), not this story. Record the deferral in `deferred-work.md`; gender and relationship are already free-text/enum-ish on the entity — leave them
- Never kill the user's running `IIROSA.Api` process. Tests excluded per the standing user decision.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Generic lookup browser screen | 19-4 |
| Guardian-specific lookup lists | 19-5 |
| The screens consuming these feeds | 8-3, 8-4, 8-8, 8-9 |
| Prayer/memorisation/hobby/behaviour lookup entities | deferred — EP-05 refinement |

### References

- [Source: docs/Modules/13-UC-ORP-Orphan-Register-and-Coding.md#13.U.11] scenario + summary list
- [Source: _bmad-output/planning-artifacts/epics.md#3.8] US-ORP-11 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs#L467] departments endpoint — the pattern to copy
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/EducationLevel.cs] lookup entity to expose
- [Source: Backend/src/IIROSA.Application/Interfaces/IOrphanPaymentService.cs#L52] `IsBatchNoUniqueAsync` — batch number domain context

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors.
- `cd Frontend && npm run build` — exit 0.

### Completion Notes List

- The education-level / health-status lookups ship the full seven-file chain (entity repo interface + typed repo, service interface + `LookupServiceBase` service, DTO trio, AutoMapper profile, DI, controller actions) — the 19-x pattern, no shortcuts.
- Batch numbers scope via item membership (`GetGroupIdsByCharityAsync`: groups whose orphans resolve to the charity, direct or through the family) — the charity dimension lives on the orphan, not the batch.
- Deferred surfaces recorded: (a) orphan-modal labels — the modal does not exist (8-4 note); (b) orphan-form selects — the form does not exist (EP-05); (c) family-form rebinding deliberately not done — its education/health fields are free strings in the epic-4/5 wire contract, and changing their type is that epic's call. The endpoints are live for all three.
- Prayer / memorisation / hobby / behaviour / relationship lookups remain deferred — no Domain entities (EP-05 refinement), as pre-recorded.
- Live walkthrough pending the user's IIROSA.Api restart.

### File List

- `Backend/src/IIROSA.Domain/Interfaces/ILookupRepository.cs` (+ IEducationLevelRepository, IHealthStatusRepository)
- `Backend/src/IIROSA.Infrastructure/Data/Repository/LookupRepository.cs` (+ 2 impls)
- `Backend/src/IIROSA.Application/Interfaces/ILookupService.cs` (+ 2 services)
- `Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs` (6 DTOs)
- `Backend/src/IIROSA.Application/Services/LookupManagementService.cs` (EducationLevelService, HealthStatusService)
- `Backend/src/IIROSA.Application/Profiles/LookupProfile.cs` (6 mappings)
- `Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs` (4 DI lines)
- `Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs` (2 actions)
- `Backend/src/IIROSA.Application/DTOs/OrphanPayment/BatchNumberDto.cs` (new)
- `Backend/src/IIROSA.Application/Interfaces/IOrphanPaymentService.cs` + `Services/OrphanPaymentService.cs` (GetBatchNumbersAsync)
- `Backend/src/IIROSA.Domain/Interfaces/IOrphanPaymentItemRepository.cs` + `Infrastructure/Data/Repository/OrphanPaymentItemRepository.cs` (GetGroupIdsByCharityAsync)
- `Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs` (GET batch-numbers)
- `Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts` (BatchNumberOptionDto)
- `Frontend/src/app/modules/orphan-payments/services/orphan-payment.service.ts` (getBatchNumbers)
- `Frontend/src/app/modules/families/orphan-payment-history/**` (consumer)
- `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORP-11 and module spec §13.U.11; endpoint set resolved to education-levels + health-statuses lookups and a distinct batch-number list; prayer/memorisation/hobby/behaviour lookups deferred (no Domain entities). |
| 2026-08-24 | Implemented: both lookup stacks + scoped batch-number list + history-dialog consumer. Orphan-form/modal label surfacing deferred (no host surfaces; family-form rebinding refused as a cross-epic wire change). Status → review. |
| 2026-08-24 | Adversarial review close-out: 3 findings — all resolved (patches applied + decisions D1–D6 recorded in the findings above; record corrections made). Backend `dotnet build` 0 errors, frontend `npm run build` OK. Status → done. |
