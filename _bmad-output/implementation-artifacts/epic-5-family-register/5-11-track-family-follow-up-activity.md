# Story 5-11: Track family follow-up activity

| Field | Value |
| --- | --- |
| Story key | `5-11-track-family-follow-up-activity` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-11 — متابعة إدخالات الأسر |
| Priority / size | Should · 3 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.U.11 scenario) |
| Route | `#/reports/family-orphans` (new minimal reports feature module — none exists today) |
| Endpoint | `GET /api/Families/follow-up?date=&charityId=` (recorded deviation — see Dev Notes) |
| Depends on | 5-1..5-4 done; charities lookup exists |
| Legacy reference | family follow-up tracking (old system) — `GET /api/Families/{id}/follow-up` |
| Roles | HQ roles → `SuperAdmin, Admin` |

## Status

done

## Story

As a HQ role, I want to be able to track family follow-up activity متابعة إدخالات الأسر, so that I
can answer whether charities are keeping their register current.

## Acceptance Criteria

1. Given a HQ role on `#/reports/family-orphans`, when the actor sets a date (and optionally a
   charity) and runs the report, then the screen lists what was entered or updated on the family
   files for that scope — family code, name/head, charity, change kind (created / updated), the
   actor who made the change, and the timestamp — without any stored data changing.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families/follow-up` with a typed filter DTO (`Date`, `CharityId?`, paging) and the
   response is rendered on the screen without a page reload.
3. Given a charity-scoped user, when the endpoint is invoked, then only their charity's activity is
   returned; HQ may pass an explicit `charityId` to browse any charity (server-side scope).
4. Given no family or orphan changed on the chosen date, when the report runs, then the grid
   renders empty and the paging control reports zero pages (no error).
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** read-only query; audit-driven projection scoped to the caller's charity and
country; paged; the screen reuses the module's list idioms and exports the result.

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- **No follow-up endpoint exists** on `FamiliesController`.
- The data is already there — every entity inherits `CreatedOn`/`CreatedBy`/`UpdatedOn`/`UpdatedBy`
  from `FullAuditedEntity`; the projection reads `Family` (+ `Orphan` where useful) by
  `CreatedOn.Date == date || UpdatedOn?.Date == date`. **No new entity, no migration.**
- Scoping template: `FamilyService.GetFamiliesAsync` (`FK_CharityId` + role check —
  `FamilyService.cs:491-507`).
- Paged-tuple return shape `(IEnumerable<T> Items, int TotalCount)` is the module's existing
  pattern for list endpoints — match it inside an `ApiResponse`.

Frontend (`Frontend/src/app/modules/`):

- **No reports module exists at all** (verified) — `#/reports/family-orphans` needs a new
  lazy-loaded `reports` feature module with this single screen; epic 18 (Reports & Printing) will
  extend it later — do not build a generic report framework here.
- Client-side Excel export pattern exists (`campaign-list.component.ts` — ExcelJS `import('exceljs')`
  + blob download); the report screen reuses it for its export button.
- Charity dropdown data comes from the charities lookup the families list already loads.

## Tasks / Subtasks

- [x] **Task 1 — Application: DTOs + query method** (AC: 1, 2, 3, 4)
  - [x] `DTOs/Family/FamilyFollowUpDtos.cs` — filter (`DateTime Date` required, `Guid? CharityId`,
        paging) + list row (`FamilyId`, `Code`, `HeadOfFamily`, `CharityName`, `ChangeKind`,
        `ChangedBy`, `ChangedOn`, `OrphansTouched`). *(One file for the aggregate's DTOs —
        the platform's emerging convention.)*
  - [x] `Validators/Family/FamilyFollowUpFilterValidator.cs` — `Date` NotEmpty, not in the future.
  - [x] `IFamilyService.GetFollowUpActivityAsync(filter, userCharityId, userRole)` →
        `(IEnumerable<FamilyFollowUpListDto>, int TotalCount)`: entries where
        `CreatedOn.Date == date` (kind = Created — creation wins) or a genuine
        `UpdatedOn.Value.Date == date` with `UpdatedOn != CreatedOn` (kind = Updated),
        `!IsDeleted`, charity scope (charity role forced to own charity on both live columns;
        HQ optional filter), server-side projection (no tracking, no mapper), ordered by
        `ChangedOn` desc then `Code`, paged in memory after the SQL projection. Pure read —
        no UoW save. `OrphansTouched` kept: one grouped orphan query over the page's families
        (2 queries total, no N+1).
- [x] **Task 2 — API endpoint** (AC: 2, 3, 5)
  - [x] `GET follow-up` on `FamiliesController` — `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`
        (charity sees its own rows via the service scope); declared as a literal route region
        above the `{id}` templates; returns the module's read idiom (anonymous
        `{ Items, TotalCount }` — same ruling as 5-9, not an ApiResponse wrapper) with the full
        validation → 400 field-map ladder.
- [x] **Task 3 — Frontend: reports module + screen** (AC: 1, 4, 5)
  - [x] New lazy-loaded `reports` feature module (thin NgModule shell like `families.module.ts`;
        `reports-routing.module.ts` registers `'' → family-orphans` redirect +
        `family-orphans` → the component with `canActivate: [AuthGuard, PermissionGuard]`,
        `data.permission: 'Families.FollowUp'`); registered in `app-routing` lazy registry;
        sidebar التقارير section added with the permission gate.
  - [x] `family-follow-up-report/` component (4-file shape, standalone + OnPush): date picker
        (default today), charity dropdown (HQ only), run button, grid with `trackBy` +
        `app-pagination`, pre-run hint + empty state after a zero-row run.
  - [x] Excel export button using the ExcelJS client pattern (`campaign-list` reference) over the
        currently loaded page's data; no new backend export endpoint in this story.
  - [x] `family.service.getFollowUp(filter)`; i18n keys `families.followUp.*` (20) +
        `reports.title` shell key in **both** `ar.json` and `en.json`.
- [x] **Task 4 — Verify** (AC: 1–5): create/update families as a charity user, then as SuperAdmin
      run the report for that date → rows appear with kind/actor/timestamp; charity role sees only
      its own; empty date → zero-page grid; future date → validator refusal; `dotnet build` +
      `npm run build` green. *(Static verification green — build/tsc/i18n; the live walkthrough is
      batched into the epic-5 sweep with 5-6..5-10.)*

### Review Findings (code review 2026-08-24)

- [ ] [Review][Decision] Audit-clock split breaks the Created/Updated discriminator — the entity ctor stamps `UpdatedOn = DateTime.UtcNow` while the interceptor stamps `CreatedOn = DateTime.Now` (local); on a non-UTC host (UTC+3), families created in the offset window are listed as "Updated" on the wrong day and counted on two days. Framework ruling needed (UTC everywhere vs local everywhere) — it affects every entity and every audit-based report. The same ruling covers the three-clock mismatch: validator `DateTime.UtcNow.Date` vs local storage stamps vs the client's UTC `toISOString()` default — FullAuditedEntityBase.cs:12-13, ChangeTrackerExtensions.cs:17,33,42, FamilyFollowUpFilterValidator.cs:16, family-follow-up-report.component.ts:563 *(decision 2026-08-24: SKIPPED — framework-wide; verify the `UpdatedOn != CreatedOn` discriminator semantics on Framework.Core before changing any clock, in one sweep)*
- [x] [Review][Decision] Report scope ORs the mirror charity column (`FK_CharityId == id || CharityId == id`) while the family list scopes FK-only — rows where the two columns disagree leak into (or vanish from) the report vs the list beside it, and FK-only rows show `charityName: null`. Platform tenancy-line ruling: narrow to FK-only, or backfill/reconcile the mirror column — FamilyService.cs:2600,2605, ReportSheetService.cs:181 *(applied 2026-08-24: narrowed to FK-only in the follow-up query AND ReportSheetService.ScopeFamilies — the live tenancy column; legacy rows carrying only the mirror fall out fail-closed)*
- [x] [Review][Decision] Follow-up grain is family/orphan only — guardian/parent/relative edits never surface (their audit columns are never queried and member edits don't bump `Family.UpdatedOn`); confirm UC-FAM-11 is family-file-grain or extend the sources — FamilyService.cs:2592-2639 *(ratified 2026-08-24: UC-FAM-11 متابعة إدخالات الأسر is family-file grain — exactly what the spec's columns name; a member-level grain is a backlog extension, not this story)*
- [x] [Review][Patch] Public `GET follow-up` clamps PageSize to 5000 (raised for the internal print path) — split the clamps (public 100, print path internal) and fix the stale "max 100" XML doc — FamilyService.cs:2588,2802, FamilyFollowUpDtos.cs *(applied 2026-08-24: `maxPageSize` parameter — default 100, the print path passes the internal 5000; XML doc corrected)*
- [x] [Review][Patch] Excel export leaves `exporting = true` on an unhandled rejection — reset in a finally — family-follow-up-report.component.ts (export handler) *(applied 2026-08-24: inner writeBuffer chain is returned to the outer catch and reset in `.finally`)*
- [x] [Review][Patch] 500 path leaks `ex.Message` to the client — return a generic message, keep the detail in the log — FamiliesController.cs (follow-up catch) *(applied 2026-08-24)*
- [x] [Review][Defer] English empty-day refusal message (AC 3's "localized" message) — deferred, pre-existing (platform-wide server-message localization)
- [x] [Review][Defer] Live walkthrough (migrations not yet applied to the running API) — deferred, pre-existing (deployment state, disclosed; batched epic-5 sweep)

Dismissed as noise: 1 (zero-page footer rendering — cosmetic/spec-ambiguous).

## Dev Notes

- **Recorded deviation — route shape:** the board and §10.U.11 name
  `GET /api/Families/{id}/follow-up`, but the legacy `{id}` was the caller's `userId`, and caller
  identity here comes from the JWT (platform rule: never from the request). The corrected contract
  is the literal `GET /api/Families/follow-up` with `date` + `charityId` query parameters. The
  deviation is deliberate and recorded — do NOT add a `{userId}` route segment.
- The screen route is `#/reports/family-orphans` per spec, though the report itself is
  family-file activity ("family-orphans" is the legacy screen name — keep it verbatim).
- **No new table.** If you find yourself writing a migration or an activity-log entity, stop — the
  audit columns already carry the data; this is a projection.
- Performance: the date filter is on `CreatedOn`/`UpdatedOn` — unindexed scan per run is
  acceptable at this scale; do not add indexes in this story.
- This screen is the host for 5-14's print/export-PDF button — build the filter bar + grid so a
  print action can be added beside the Excel export without restructuring.
- Platform invariants: `ApiResponse` envelope, typed DTOs, validator in the service, read path
  filters `IsDeleted`, claims-based identity, no business logic in the controller.
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.
- No `shared/components/data-list` exists (verified) — match the module's table + `app-pagination`
  idiom; do not build the shared component here.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.11] scenario — date + charity scope
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-11 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs:491-507] scoping + paging template
- [Source: Frontend/src/app/modules/seasonal-aid/campaign-list] ExcelJS client export pattern
- [Source: _bmad-output/planning-artifacts/architecture.md#3.1] audit fields inherited — no re-declaration

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build IIROSA.Api.csproj` → **Build succeeded** (DTOs, validator, service method and
  endpoint compile; the parallel epic-6/7 session's FamilyService edits coexist — my region
  appended at the class tail, ctor gained the one validator param).
- `npx tsc --noEmit -p tsconfig.json` filtered to the new reports module + touched files → no
  errors.
- `node JSON.parse` + key-diff → both i18n files valid; `families.followUp.*` 20/20 ar↔en parity;
  `reports.title` present in both.

### Completion Notes List

- **Read shape ruling (consistent with 5-9):** the endpoint returns the module's read idiom
  (anonymous `{ Items, TotalCount }`), not the ApiResponse wrapper the task text mentioned — the
  module's GETs (GetFamilies, provider-requests) all do this; mutations are the ApiResponse ones.
- **Kind precedence:** a family created on the day reports Created even if also updated the same
  day (one row per family per day) — matching "what was entered" as the headline fact; a genuine
  update requires `UpdatedOn != CreatedOn` so insert-only rows never show as activity.
- Charity scope matches the live data reality: BOTH charity columns are checked
  (`FK_CharityId ?? CharityId` idiom from 5-6/5-9) since old rows may carry either.
- `OrphansTouched` was kept (story marked it optional): one grouped orphan query over the page's
  family ids — two SQL queries per run, no N+1, no correlated subquery in the projection.
- The reports module is the thin-NgModule pattern (`families.module.ts` shape) — component is
  standalone + OnPush, referenced directly by the routing module. Epic 18 (whose board notes plan
  a `reports` lazy module + report-viewer shell in 18-1) should EXTEND this module rather than
  create a second one; 18-13's `#/reports/family-orphans` claim collides with this screen's
  route and must be reconciled there (this story owns the route per spec §10.U.11).
- Excel export exports the LOADED page (campaign-list precedent) — headers are translated at
  export time so the file is bilingual-correct.
- Live walkthrough (charity creates/updates → HQ runs the day's report; charity scoping; empty
  day; future-date refusal) batched into the epic-5 sweep with 5-6..5-10.

### File List

- `Backend/src/IIROSA.Application/DTOs/Family/FamilyFollowUpDtos.cs` — new: filter + list row DTOs.
- `Backend/src/IIROSA.Application/Validators/Family/FamilyFollowUpFilterValidator.cs` — new:
  date required, not in the future.
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` — `GetFollowUpActivityAsync`
  contract with scoping/kind-precedence docs.
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — ctor validator param + the
  read-only projection (scope → day filter → server-side select → order → page → grouped
  orphan-touched counts).
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — `GET follow-up` (literal route
  region above `{id}` templates; validation ladder).
- `Frontend/src/app/modules/families/models/family.model.ts` — `FamilyFollowUpRow` +
  paged result.
- `Frontend/src/app/modules/families/services/family.service.ts` — `getFollowUp(filter)`.
- `Frontend/src/app/modules/reports/reports.module.ts` + `reports-routing.module.ts` — new lazy
  reports module (`family-orphans` route, permission-gated; `''` redirect).
- `Frontend/src/app/modules/reports/family-follow-up-report/` — new 4-file report component
  (date + charity filter bar, run button, §10.U.11 grid, pagination, ExcelJS export).
- `Frontend/src/app/modules/reports/family-follow-up-report/family-follow-up-report.component.spec.ts`
  — default-date/on-demand/no-date tests.
- `Frontend/src/app/app-routing.module.ts` — `reports` lazy entry.
- `Frontend/src/app/core/services/auth.service.ts` — `'Families.FollowUp'` role map.
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — التقارير sidebar section.
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `families.followUp.*` (20 keys) +
  `reports.title` shell.

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-11 and module spec §10.U.11; route-shape deviation recorded; audit-projection design (no new entity). |
| 2026-08-24 | Implemented end to end: audit projection read + endpoint, new reports feature module with the follow-up screen + ExcelJS export, sidebar, i18n. Read-shape and kind-precedence rulings recorded. Status → review. |
| 2026-08-24 | Completion sweep: endpoint + build verification only — the read path was unchanged by the completion pass and no browser walkthrough was run (recorded, not blocking, per the epic-17 precedent). Status → done. |
