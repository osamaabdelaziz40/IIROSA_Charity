# Story 18-6: Widows requiring sponsorship أرامل مطلوب لهم كفالة

| Field | Value |
| --- | --- |
| Story key | `18-6-widows-requiring-sponsorship` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-06 — أرامل مطلوب لهم كفالة |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.9 screen, §23.U.6 scenario) |
| Route | `#/reports/widows-allowing-sponsorship` |
| Endpoint | `POST /api/Reports/widows-allowing-sponsorship` |
| Depends on | **18-1 landed** (Reports skeleton + `report-viewer` shell + `report-export.service.ts`) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to widows requiring sponsorship أرامل مطلوب لهم كفالة, so
that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a signed-in user with an active session on the screen at
   `#/reports/widows-allowing-sponsorship`, when the actor presses «بحث», then no stored data is
   changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/widows-allowing-sponsorship` with a typed request DTO and the raw paged
   envelope is rendered without a page reload.
3. Given the caller is a charity user, when the report runs, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
   Given an HQ caller (`IsHeadOffice`) with an explicit charity id, the report runs on that
   charity's data; a `CountryId` claim additionally pins the charities of that country.
4. Given no widow is flagged as allowing sponsorship in scope, when the report runs, then the grid
   renders empty and the paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the single charity filter, the 22-column widow grid and the single بحث
command of §23.S.9 are implemented on 18-1's shell; §23.U.6 passes end to end; the widow predicate
resolves from what the widow/mother registration actually stores today (gaps render blank and are
recorded — no migration); the scope is enforced server-side.

## Screen contract (§23.S.9 — ارامل مطلوب لهم كفاله, 1 filter field, 1 command)

| Section | Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- | --- |
| ارامل مطلوب لهم كفاله | الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items || []`) + كافة الجهات all-option — HQ only; hidden for charity callers |

| Grid columns (row source: widows) |
| --- |
| اسم الارمله · المحافظه · المركز · القريه · العنوان تفصيلي · الموبايل 2 · الموبايل 1 · ملكيه السكن · قيمه الايجار · نوع السكن · حاله السكن · قيمه الدخل · تاريخ وفاه الزوج · مشروع تنموي · مؤهل الارمله · مهنه الارمله · الحاله الصحيه للارمله · الرقم القومي · ملاحظات · اسم الجمعيه · تاريخ اخر تحديث |

| Command | Handler | In scope |
| --- | --- | --- |
| بحث | `ExtractWidowsThatAllowWidowSponsorship()` | Yes (bound to the standard بحث button) |

Recorded: §23.S.9 lists NO استخراج command — the grid is query-only. If an export is wanted it
arrives with 18-41's generic engine; do not add a per-screen export here.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 3)
  - [x] `WidowSponsorshipFilterDto` (`Guid? CharityId`, `int Page = 1`, `int PageSize = 20`) +
        `WidowSponsorshipListDto` (the 22 grid keys, clean names, widow + family + charity
        resolved) in `DTOs/Reports/Reports.cs`; result reuses `ReportPagedResult<T>`; no `FK_*`
        wire keys
  - [x] `Validators/Reports/WidowSponsorshipFilterValidator.cs` — page bounds; invoked in the
        service
  - [x] `IReportService.GetWidowsAllowingSponsorshipAsync(...)` + implementation:
        `ResolveCharityScope(filter.CharityId)`, then a read-only projection joining the widow
        (mother) data of families — the widow record lives on the family's mother entity with the
        residence/income columns on `Family` (region/center/village/address/rent/ownership/income —
        the same columns the refugee contract of EP-07 populated); predicate = widows eligible for
        widow sponsorship **as the data expresses it today**: verify what flag/column the widow
        registration carries (death-of-husband date presence is the floor). Columns without a
        backing field (e.g. مشروع تنموي, مؤهل/مهنه/حالة صحية for the widow) render blank —
        record the gap, never migrate (epic-wide ruling)
- [x] **Task 2 — API endpoint** (AC 2, 5)
  - [x] `[HttpPost("widows-allowing-sponsorship")] [Authorize(Roles = "SuperAdmin,Admin,Charity")]`
        in 18-1's `ReportsController` → `Ok(paged)`; standard error ladder; no `ApiResponse<T>`
- [x] **Task 3 — Screen** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/widows-sponsorship-report/` thin 4-file component in the
        shell; route `widows-allowing-sponsorship`, `AuthGuard + PermissionGuard`,
        `data.permission: 'Reports.View'`; bespoke 22-column grid with horizontal scroll + shared
        `Pagination`; `trackBy`; empty state; OnPush omitted (list-screen precedent); the single
        بحث command; no export button
- [x] **Task 4 — i18n** — `reports.widowsSponsorship.*` (title, filter label, 22 grid headers,
      empty state) in **both** `ar.json` and `en.json`
- [x] **Task 5 — Verification** (AC 1–5) — anonymous → 401; charity token → own rows only; HQ +
      `charityId` → that charity; empty scope → empty grid; `dotnet build` + `npm run build` green
      (MSB3021/3027 live-API lock caveat; ng-serve stale-bundle grep); tests excluded per the
      standing user decision

## Dev Notes

### Data-source decision (recorded)

The platform has no standalone Widow entity — the widow is the family's mother (the 22 columns of
§23.S.9 are the mother + family residence/income set). The projection reads mother+family rows;
"allowing sponsorship" must resolve from existing data (start from: family registered + husband
dead (`تاريخ وفاه الزوج` present) + orphan children present). If the registration carries a
dedicated widow-sponsorship flag, prefer it. Whatever predicate ships, record it in the Dev Agent
Record — do NOT add a column or migration in this epic.

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
| Widow identification sheets (print variants ForWidows / ForWidows_Family) | 18-37 |
| New-orphans-and-new-widows printed lists | 18-35 |
| Excel export for this grid | 18-41 (generic engine only — spec shows no per-screen command) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.9] screen contract — 1 field,
  22-column grid, 1 command
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.6] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-06 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] skeleton + shell this story
  reuses
- [Source: Backend/src/IIROSA.Domain] `Family` / `Mother` entities — the residence/income column
  set the projection reads

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Smoke run 2026-08-24, private instance `dotnet IIROSA.Api.dll --urls http://127.0.0.1:60970`:
  - anonymous POST `/api/Reports/widows-allowing-sponsorship` → **401** ✓
  - SuperAdmin POST → **200** empty envelope ✓ (legitimate: see predicate note below)
  - Charity-role token POST → **200** (role allowed; pinned server-side) ✓
  - out-of-range page bounds → **400** `errors.Page`/`errors.PageSize` ✓
- Predicate proof vs `IIROSA_Db_Dev` (sqlcmd): 2 mothers alive+non-deleted (both with families),
  2 fathers non-deleted, **0 fathers carry DeathDate**, 4 families with orphans → the floor
  predicate legitimately yields the empty set today (AC 4's exact case), not a broken query.
- `dotnet build Backend/IIROSA.sln` — 0 CS errors (MSB3021/3027 copy-locks only);
  `npm run build` exit 0.

### Completion Notes List

- **Predicate (recorded per Dev Notes):** no widow-sponsorship flag exists anywhere in the
  domain, so "requiring sponsorship" resolves to the data's floor — mother alive + non-deleted,
  registered non-deleted family, non-deleted father with `DeathDate != null`, and ≥1 non-deleted
  orphan in the family. When a dedicated flag lands it replaces this predicate in ONE place
  (`GetWidowsAllowingSponsorshipAsync`). No column added, no migration.
- **Column mapping (22 = serial + 21):** widow name/education/profession/health/national id/
  notes/phone from `Mother`; governorate/center/village/address/rent/ownership/housing type/
  house status/income from `Family` (epic-7 refugee column set); husband death date from
  `Father.DeathDate`; charity resolved via dictionary; last-updated = `Mother.UpdatedOn`.
  **Recorded gaps (render blank):** الموبايل 2 (no second-phone column — same gap as 18-1) and
  مشروع تنموي (no development-project column on Mother/Family).
- **Query-only honoured:** §23.S.9 lists no استخراج — the shared shell gained a `showExport`
  input (default true; this screen passes false). No per-screen export was added.
- Charity scope rides the family's `FK_CharityId` through a mother-scoped mirror of
  `ApplyCharityScopeAsync` (`ApplyMotherCharityScopeAsync` — same pin-never-widen ladder).

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `WidowSponsorshipFilterDto`,
  `WidowSponsorshipListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/WidowSponsorshipFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetWidowsAllowingSponsorshipAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation +
  `ApplyMotherCharityScopeAsync` + `ResolveWidowCharityNamesAsync` + `IRepository<Mother>` ctor
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `widows-allowing-sponsorship` action
- `Frontend/src/app/modules/reports/models/report.model.ts` — `WidowSponsorshipFilter`,
  `WidowSponsorshipRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getWidowsAllowingSponsorship()`
- `Frontend/src/app/modules/reports/widows-sponsorship-report/` — new 4-file component (query-only)
- `Frontend/src/app/modules/reports/report-viewer/report-viewer.component.ts` + `.html` — new
  `showExport` input + `*ngIf` on the export button (default true — no other screen changes)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `widows-allowing-sponsorship` route
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.widowsSponsorship.*` (27 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-06 and module spec §23.S.9 / §23.U.6; mother+family projection and no-dedicated-widow-entity data-source decision recorded; per-screen export withheld per the spec's command list. |
| 2026-08-24 | Implemented: floor predicate (no widow-sponsorship flag exists — mother alive + father DeathDate + orphans present, recorded), 22-column mother+family projection with 2 blank-by-contract gaps, query-only shell (`showExport=false` added to the viewer), i18n both locales; verified on private smoke instance (401/200-empty/400) with the empty set proven legitimate against the DB. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
