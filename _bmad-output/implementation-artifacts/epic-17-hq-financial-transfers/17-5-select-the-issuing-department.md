# Story 17-5: Select the issuing department

| Field | Value |
| --- | --- |
| Story key | `17-5-select-the-issuing-department` |
| Epic | EP-17 — HQ Financial Transfers (الحوالات المالية للادارة المالية) |
| Use case | UC-TRF-05 — الإدارة المصدرة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md` (§22.U.5 scenario) |
| Route | — (no screen of its own; feeds the §22.S.2 form's اسم الادارة الطالبة dropdown) |
| Endpoint | `GET /api/LookupManagement/departments` — **already exists** |
| Depends on | **17-2 landed** (the form that consumes the dropdown) |
| Roles | Fin. Director → `SuperAdmin`, `Admin` — any authenticated caller may read lookups |

## Status

done

## Story

As a Financial Director, I want to be able to select the issuing department الإدارة المصدرة, so
that the transfer I record is attributed to the head-office department that owns it.

## Acceptance Criteria

1. Given a Financial Director with an active session, when the department list is requested, then
   no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/departments` and the response is rendered in the transfer form's
   اسم الادارة الطالبة drop-down without a page reload.
3. Given a department is deactivated, when a **selection screen** requests the active-only list
   (the transfer form passes `isActive: true`), then it is not offered for new selection (existing
   transfers keep their historical name resolution). *Amended 2026-08-24 per product ruling: the
   endpoint serves what callers ask for — the lookup admin screen depends on seeing inactive rows —
   so the active-only guarantee sits at the caller, not the endpoint default.*
4. Given the session has expired, when the function is invoked, then the request is rejected and
   the actor is routed back to the login screen.

**Definition of done:** the dropdown on `#/hq-transfers/create` and `#/hq-transfers/:id/edit` is
driven by the live endpoint with Arabic-first labels, inactive rows excluded, and the full
catalogue reachable (not truncated by paging).

## What exists already (verified — DO NOT rebuild)

The endpoint is LIVE and shape-verified (`LookupManagementController.cs:462`):

```csharp
[HttpGet("departments")]
public async Task<ActionResult<LookupPagedResult<DepartmentDto>>> GetDepartments(
    [FromQuery] LookupFilterDto filter)
// → _departmentService.GetLookupItemsAsync(filter); try/catch → 500 { message }
```

- `LookupFilterDto` (`DTOs/LookupManagement/LookupDtos.cs:385`): `SearchText`, `IsActive`,
  `Page = 1`, `PageSize = 20` (+ `CountryId`/`RegionId` used by other lookups).
- `Department : LookupEntity` exists (`Entities/Lookups/Department.cs`) with `Description`.
- Epic 16's story 16-8 (correspondence routing department) consumes the SAME endpoint — anything
  fixed here serves both epics; do not fork a second departments endpoint.

This is a **verification + wiring-quality story** in the 15-3/15-4/15-5 class: the endpoint
exists; the story's job is to prove the contract serves §22.S.2 and close any gap.

## Tasks / Subtasks

- [x] **Task 1 — Audit the read contract** (AC 1, 2, 3)
  - [x] Live-check `GET /api/LookupManagement/departments`: 200 with camelCase
        `items/totalCount/page`; each item carries `id`, `nameAr`, `nameEn`, `isActive`
  - [ ] Verify `GetLookupItemsAsync` actually filters `IsActive == false` rows out when no
        explicit `IsActive` filter is passed — read `DepartmentService`/lookup service
        implementation; if inactive rows leak into the default list, fix the DEFAULT to
        active-only (callers opt into `IsActive=false` explicitly) — that is this story's one
        permissible backend change
  - [x] Confirm no duplicate departments-capability endpoint exists (the known debt pairs are
        `LookupManagementService` / `LookupManagementManagementService` — if the departments read
        is served by the duplicate, note it for the retro; do NOT refactor the pair in this
        story)
- [x] **Task 2 — Dropdown quality on the transfer form** (AC 2, 3)
  - [x] The form's اسم الادارة الطالبة select loads from the endpoint with `pageSize: 100` (or
        the largest sensible page) so the catalogue is not truncated at the default 20 — verify
        what 17-2 shipped and correct if needed
  - [x] Option label `nameAr ?? nameEn`; option value `id`; empty placeholder option
        (اختر الإدارة…) so a mandatory unset state is visible
  - [x] Failed lookup load leaves the field selectable-but-empty with a retry, not a silent empty
        list
- [x] **Task 3 — i18n** — placeholder, load-failure and retry labels under `hqTransfers.*` in
      **both** `ar.json` and `en.json`
- [x] **Task 4 — Verification** (AC 1–4)
  - [ ] Live: unauthenticated → 401; authenticated → 200; deactivated department absent from the
        default list; `SearchText` narrows
  - [x] UI: create + edit forms offer the full active list; saving with a department selected
        round-trips `departmentName` on the list/view screens
  - [x] `dotnet build` + `npm run build` green (lock caveats as usual)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Why this story is small

The heavy lifting (endpoint, service, entity, DTO) predates the epic — UC-TRF-05's
"realisation" row in the module doc even names `LookupManagementController`/`ILookupService`
rather than the transfers controller. The story exists to (a) prove the contract against
§22.S.2's mandatory dropdown, (b) close the inactive-row and paging-truncation gaps, and
(c) leave a verified trail for epic 16's 16-8, which shares the endpoint.

### Platform rules that bind this story

- Lookup labels resolve `NameAr ?? NameEn` everywhere (Arabic primary).
- No new endpoint, no new service, no DTO fork — reuse is the requirement, not an option.
- Raw envelope; camelCase wire; no `ApiResponse<T>` wrapper.

### Out of scope

| Item | Owner |
| --- | --- |
| Wiring the departments dropdown into the correspondence screens | epic 16, story 16-8 |
| Resolving the duplicate `LookupManagement*Service` pair | recorded platform debt (retro candidate) |
| Department CRUD screens | lookup-management module's own backlog, not UC-TRF-05 |

### References

- [Source: docs/Modules/22-UC-TRF-HQ-Financial-Transfers.md#22.U.5] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.17] US-TRF-05 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs:462] the live
  departments endpoint (verified 2026-08-24)
- [Source: Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs:385]
  `LookupFilterDto` defaults
- [Source: Backend/src/IIROSA.Domain/Entities/Lookups/Department.cs] the lookup entity
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-3-select-the-mission-type.md … 15-5] the
  reviewed lookup-contract story class this follows

### Review Findings

_From the epic-17 full-epic review (2026-08-24 — blind/edge/acceptance layers over all 8 stories)._

- [x] [Review][Decision] AC 3 quietly narrowed — **ruled 2026-08-24: amend the AC** (epic-5
      precedent). AC 3 above now reads "selection screens request active-only" — the endpoint
      keeps serving what callers ask for (the lookup admin screen depends on seeing inactive
      rows); the transfer form already passes `isActive: true`. No code change — resolved by the
      amendment.

## Dev Agent Record

### Agent Model Used

Claude (Claude Code CLI, glm-5) — 2026-08-24.

### Debug Log References

- **Live re-checks deferred (again on user WIP)** — during this story's window the user's
  concurrent rewrite moved from `PeriodicOrphanReportService`/`Family` to
  `OrphanPaymentService.cs` (3 errors, single file, converging). No runnable API binary for
  the 401/200/deactivated-absent/SearchText battery; the endpoint itself was live-verified at
  story-creation time (see Change Log) and exercised end to end by 17-2's create battery
  (the form's catalogue load IS this endpoint — the saved record resolved
  `departmentName` "وكيل الشؤون التنفيذية" from departmentId 100).
- Frontend: `npm run build` **0 errors** after this story's edits (hq-transfers chunk rebuilt).
- Backend: **0 files touched** — solution red only in the user's `OrphanPaymentService.cs` WIP
  (unrelated to lookups).

### Completion Notes List

1. **Default-list inactive-row leak CONFIRMED, deliberately NOT fixed** (unticked Task 1 box).
   `LookupServiceBase.GetLookupItemsAsync` (`LookupManagementService.cs:59-62`) filters
   `IsActive` only `if (filter.IsActive.HasValue)` — inactive rows DO leak into a no-filter
   list. Flipping the default to active-only was rejected with evidence: the lookup-management
   admin screen (`lookup-management/departments/departments-list.component.ts:56-66`) sends NO
   `isActive` for its default "All" view and depends on seeing inactive rows (status badges,
   activate/deactivate actions) — flipping would silently hide them from admins. Every form
   consumer (charities, housing, correspondence, hq-transfers) already passes
   `isActive: true` explicitly, so the leak is theoretical for §22.S.2 and load-bearing for
   the admin UI. Recorded as a retro candidate: the real fix is a query-expressible tri-state,
   not a default flip.
2. **`Name` on the wire is computed, culture-aware** — `LookupEntityBase.Name`
   (`Framework.Framework.Core/Data/EntityBase.cs:59`) returns `NameAr` when the request
   culture is Arabic else `NameEn`; the Department table has no `Name` column (verified via
   `sqlcmd` — the column query failed, which surfaced the computed property). So
   `CreateMap<Department, DepartmentDto>()` (convention-only) emits an Arabic-first label
   server-side. The form's option labels were made defensive anyway —
   `name || nameAr || nameEn` (mission-form precedent) — to guard a null `NameEn` under an
   English-culture request.
3. **Duplicate service pair audited** — the departments read is served by the canonical
   `DepartmentService : LookupServiceBase` via `IDepartmentService`
   (`LookupManagementController.cs:487`); `LookupManagementManagementService` is not in this
   path. Stays on the recorded-debt list (CLAUDE.md); no refactor, per the story.
4. **17-2 had already shipped most of Task 2** — `pageSize: 1000` + `isActive: true` on both
   catalogues and the `selectDepartment` placeholder. This story added the one missing piece:
   the failure affordance — `catalogueFailed` flag + inline warning banner with an
   إعادة المحاولة retry button (`retryCatalogues()`), replacing the toast-only behaviour.
5. **AC 4 (401 → login redirect)** is the platform `AuthGuard`/HTTP-interceptor behaviour
   shared by every module; not re-probed on this endpoint (see Debug Log).

### File List

**Backend:** none — audit-only story; the endpoint, service, and DTOs predate the epic and
were verified, not modified.

**Frontend:**
- `Frontend/src/app/modules/hq-transfers/hq-transfer-form/hq-transfer-form.component.ts` —
  `catalogueFailed` state, `retryCatalogues()`, failure handler sets the flag (load resets it)
- `Frontend/src/app/modules/hq-transfers/hq-transfer-form/hq-transfer-form.component.html` —
  inline retry banner above the form; defensive `name || nameAr || nameEn` option labels
  (country + department)
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `hqTransfers.retryLoad`
  (إعادة المحاولة / Retry)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-TRF-05 and module spec §22.U.5; endpoint verified live — scoped to contract audit + dropdown quality. |
| 2026-08-24 | Audit completed (leak found, default-flip rejected with admin-screen evidence — Note 1; duplicate pair cleared — Note 3) + dropdown failure affordance shipped (retry banner, defensive labels, i18n). Live 401/200 battery deferred on user WIP; endpoint contract already evidenced at story creation and via 17-2's create round-trip. Status → review. |
