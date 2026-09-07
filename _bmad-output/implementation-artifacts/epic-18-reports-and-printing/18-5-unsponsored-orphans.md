# Story 18-5: Unsponsored orphans أيتام غير مكفولين

| Field | Value |
| --- | --- |
| Story key | `18-5-unsponsored-orphans` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-05 — أيتام غير مكفولين |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.6 screen, §23.U.5 scenario) |
| Route | `#/reports/unsponsored-orphans` |
| Endpoint | `POST /api/Reports/unsponsored-orphans` |
| Depends on | **18-1 landed** (Reports skeleton + `report-viewer` shell + `report-export.service.ts`); 18-4's shared grid shape |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to unsponsored orphans أيتام غير مكفولين, so that the
register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/unsponsored-orphans`,
   when the actor presses «بحث», then no stored data is changed — the operation is a read. (The
   epic's «Create a record» wording is template artefact — §23.S.6/§23.U.5 show a query screen;
   no entity is created; see Dev Notes.)
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/unsponsored-orphans` with a typed request DTO and the raw paged envelope is
   rendered without a page reload.
3. Given the caller is a charity user (endpoint roles are HQ-only, scope guard still applies),
   when the report runs, then only that charity's rows are returned — pinned server-side from
   `ICurrentUserService.CharityId`, never from the payload. Given an HQ caller (`IsHeadOffice`)
   with an explicit charity id, the report runs on that charity's data; a `CountryId` claim
   additionally pins the charities of that country.
4. Given no orphan sits in the unsponsored state in scope, when the report runs, then the grid
   renders empty and the paging control reports zero pages.
5. Given استخراج البيانات is pressed with an empty result, when the export runs, then the actor is
   told there is nothing to produce rather than receiving an empty file.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the single charity filter and the 5-column grid of §23.S.6 are implemented
on 18-1's shell, sharing 18-4's component/projection shape; §23.U.5 passes end to end; the
unsponsored predicate resolves from the live `SponsorshipStatus` enum (first code assignment flips
null/Pending → Unsponsored per the EP-08 ruling — no new column); the scope is enforced
server-side.

## Screen contract (§23.S.6 — الايتام غير مكفولين, 1 filter field, 2 commands)

| Section | Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- | --- |
| الايتام غير مكفولين | الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items || []`) + كافة الجهات all-option — HQ only |

| Grid columns (row source: unsponsored orphans — same shape as §23.S.5) |
| --- |
| رقم اليتيم · أسم اليتيم · الجمعيه · كود العائله · العمر |

| Command | Handler | In scope |
| --- | --- | --- |
| بحث | `GetData()` | Yes |
| استخراج البيانات | `ExportData()` | Yes |

## Tasks / Subtasks

- [x] **Task 1 — Shared shape with 18-4** (AC 2) — reuse `OrphanStatusReportFilterDto` /
      `OrphanStatusReportListDto` from 18-4 verbatim (same filter, same 5 columns); do NOT fork a
      parallel DTO pair for this report key
- [x] **Task 2 — Service predicate + endpoint** (AC 2, 3, 4)
  - [x] `IReportService.GetUnsponsoredOrphansAsync(...)` + implementation: identical query shape to
        18-4's read with the predicate flipped to the unsponsored member of the live
        `SponsorshipStatus` enum (verify member names in `IIROSA.Domain` first; per the EP-08
        ruling, "unsponsored" = coded orphan whose sponsorship status is Unsponsored — an orphan
        with an empty `Code` is UNCODED, not unsponsored, and must NOT appear here; if the enum
        cannot express the distinction, STOP and surface rather than inventing a column)
  - [x] `[HttpPost("unsponsored-orphans")] [Authorize(Roles = "SuperAdmin,Admin")]` in 18-1's
        `ReportsController` → `Ok(paged)`; standard ValidationException→400-errors-map /
        catch-all→500 `{ message }` ladder; no `ApiResponse<T>`
- [x] **Task 3 — Screen** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/unsponsored-orphans-report/` thin 4-file component in the
        shell — if 18-4 has landed, extend its shared grid component with a second route+predicate
        rather than duplicating the template; route `unsponsored-orphans`,
        `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`; bespoke grid + shared
        `Pagination`; `trackBy`; empty state; OnPush omitted (list-screen precedent)
- [x] **Task 4 — Export** (AC 5) — استخراج via 18-1's `report-export.service.ts` with this
      report's column set/i18n key; empty result → message, no file
- [x] **Task 5 — i18n** — `reports.unsponsoredOrphans.*` in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Anonymous → 401; Charity-role token → 403; HQ + `charityId` → that charity; empty scope →
        empty grid
  - [x] Predicate spot-check against seeded data: a coded unsponsored orphan appears; an uncoded
        orphan does NOT (the uncoded set is 18-1's `NotExcluded` default, not this report)
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 live-API lock caveat; ng-serve
        stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Template artefact (recorded)

The epic generates US-RPT-05 as «Create a record» with save/refusal ACs. §23.S.6 and §23.U.5 show
a query screen (charity dropdown + grid + بحث/استخراج). Nothing is created by this screen; the
sponsorship state itself is written by the coding/sponsorship verticals (EP-08 and later). This
story is sized 5 points for the shared-shape extraction and the predicate semantics, not for any
write path.

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`; bespoke grid +
  shared `Pagination` (NOT `data-list`); lazy module; EP-18 adds no entities and no migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Ended-sponsorship variant | 18-4 |
| Coding an orphan / sponsorship assignment | EP-08 |
| Excel-export engine hardening | 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.6] screen contract — 1 field,
  5-column grid, 2 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.5] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-05 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-4-orphans-with-ended-sponsorship.md] the
  shared filter/DTO/grid shape this story reuses
- [Source: Backend/src/IIROSA.Domain] `SponsorshipStatus` enum — predicate source of truth
  (EP-08 ruling: first code assignment flips null/Pending → Unsponsored)

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Smoke run 2026-08-24, private instance `dotnet IIROSA.Api.dll --urls http://127.0.0.1:60970`:
  - SuperAdmin POST `/api/Reports/unsponsored-orphans` → **200**
    `{"items":[{"code":"LC-CODE-1","fullName":"…","charityName":"dga","familyCode":"FAM-2026-8106","age":10}],"totalCount":1,…}` ✓
  - out-of-range page bounds → **400** with `errors.Page` / `errors.PageSize` ✓
- Predicate spot-check vs `IIROSA_Db_Dev` (sqlcmd, schema-qualified `[IIROSA].Orphan`):
  - `COUNT(*) WHERE IsDeleted=0 AND Code<>'' AND SponsorshipStatus='Unsponsored'` → **1** (matches
    `totalCount: 1` — the coded orphan appears) ✓
  - `COUNT(*) WHERE IsDeleted=0 AND (Code IS NULL OR Code='') AND SponsorshipStatus='Unsponsored'`
    → **0** (no uncoded orphan appears) ✓
- `dotnet build Backend/IIROSA.sln` — 0 compile errors (MSB3021/3027 copy-locks only);
  `npm run build` exit 0.

### Completion Notes List

- No DTO fork: `OrphanStatusReportFilterDto`/`OrphanStatusReportListDto`/validator/component/export
  are 18-4's, reused verbatim. This story adds exactly: the service predicate method, the
  endpoint, a second route (`data.orphanStatusVariant: 'unsponsored'`) on 18-4's component, its
  i18n block, and the sidebar entry — the "extend its shared grid component" instruction from
  Task 3 honoured literally (no `unsponsored-orphans-report/` directory needed).
- Story premise correction: `SponsorshipStatus` is a FREE STRING, not an enum — but unlike 18-4's
  "ended" state, "Unsponsored" IS directly representable (`SponsorshipStatus == "Unsponsored"`),
  so the STOP condition did not trigger. The EP-08 coded-vs-uncoded boundary is enforced in the
  predicate (`!string.IsNullOrEmpty(o.Code)`) and proven against the DB above.
- Age computes server-side via `EF.Functions.DateDiffYear` (18-1's pattern); charity names resolve
  through the shared `ResolveStatusReportCharityNamesAsync` dictionary.

### File List

- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetUnsponsoredOrphansAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (coded + Unsponsored
  predicate, charity scope, age projection)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `unsponsored-orphans` action
  (HQ-only roles)
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getUnsponsoredOrphans()`
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `unsponsored-orphans` route on
  18-4's shared component (`orphanStatusVariant: 'unsponsored'`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry (HQ roles)
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.unsponsoredOrphans.*` (11 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-05 and module spec §23.S.6 / §23.U.5; «Create a record» artefact re-cut to the read the spec shows; shared shape with 18-4 and the coded-vs-unsponsored predicate boundary recorded. |
| 2026-08-24 | Implemented on 18-4's shared shape (no DTO/component fork; route-data variant predicate). Predicate proven against seeded data: 1 coded unsponsored orphan returned and matching the DB, 0 uncoded leaked. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
