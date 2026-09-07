# Story 18-13: Family and orphan entry tracking

| Field | Value |
| --- | --- |
| Story key | `18-13-family-and-orphan-entry-tracking` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-13 — متابعة إدخالات الأسر والأيتام |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.10 screen, §23.U.13 scenario) |
| Route | `#/reports/family-orphans` |
| Endpoint | `GET /api/Families/{id}/follow-up` — **does not exist**; this story adds it to `FamiliesController` |
| Depends on | **18-1 landed** (reports skeleton: `modules/reports` shell, `ReportPagedResult<T>`, `report-export.service.ts`, `Reports.View` permission) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to family and orphan entry tracking متابعة إدخالات الأسر
والأيتام, so that I can answer the operational, compliance or financial question being asked of me.

## Acceptance Criteria

1. Given a signed-in user with an active session on `#/reports/family-orphans`, when the actor runs
   either data command, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families/{id}/follow-up` (query: `date`, `mode`) and the response is rendered on the
   screen without a page reload.
3. Given the caller is a charity user, when the function is invoked, then only that charity's entries
   are returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; a
   route id naming a different charity yields that charity's own scope, not another's.
4. Given an HQ caller (`IsHeadOffice`) naming an explicit charity id in the route, when the function
   is invoked, then it operates on that charity's data.
5. Given no family or orphan was entered on/after the report date for the selected charity, when the
   commands run, then the totals read zero and the details grid renders empty with zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** both data commands of §23.S.10 work — إجماليات (counts) and تفاصيل (rows) —
off the one endpoint; the print command is explicitly deferred to 18-37; the charity/country scope is
enforced server-side, not only in the menu.

## Screen contract (§23.S.10 — متابعة إدخالات الأسر والأيتام, 2 fields, 3 commands)

| Field (as labelled) | Bound to | Control | Mandatory · rule |
| --- | --- | --- | --- |
| تاريخ بدء التقرير | `date` | Date picker | Optional · entries on/after this date; omit → all entries |
| الجمعية | `charityId` (route `{id}`) | Drop-down | Optional · `GET /api/Charities` (`result.items \|\| []`); كل الجهات all-option for HQ; hidden/disabled for a charity caller — the server pins anyway |

| Command | Handler | This story |
| --- | --- | --- |
| إجماليات إدخالات الأسر والأيتام | `GetFamilyOrphans()` | **built** — totals (`mode=totals`): families count + orphans count entered since the date |
| تفاصيل إدخالات الأسر الجديدة | `GetFamilyOrphansDetails()` | **built** — rows (`mode=details`): كود الأسرة · رب الأسرة · الجمعية · تاريخ التسجيل · عدد الأيتام, paged |
| طباعة الاستبانة من تاريخ محدد | `PrintCharityIdentifications()` | **out of scope** — 18-37 (identification sheets); do not render the button |

## Tasks / Subtasks

- [x] **Task 1 — DTOs + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `FamilyFollowUpTotalsDto`
        (`NewFamilies`, `NewOrphans`, `SinceDate`) and `FamilyFollowUpDetailDto` (the five detail
        keys + `CharityName`, clean names — no `FK_*` wire keys)
        **— plus `FamilyEntryTrackingFilterDto` (route id + date + mode + paging) so the validator
        has one object; refinement of the story's param-list signature, recorded**
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/FamilyEntryTrackingValidator.cs` — the
        query is two optional keys (`date`, `mode` ∈ totals|details) + paging; keep it to bounds
        and the mode whitelist **(named for the filter DTO; the story's
        `FamilyFollowUpValidator` name referred to the same object)**
- [x] **Task 2 — Service method** (AC 1, 3, 4, 5)
  - [x] `IFamilyService` / `FamilyService` — `GetFamilyFollowUpAsync(...)`: scope via the
        pin-never-widen shape — a charity caller is clamped to its own charity whatever the route
        id says (the file's `GetFollowUpActivityAsync` role-string convention, not
        `ICurrentUserService`; recorded); HQ may name any charity, `Guid.Empty` → all charities
  - [x] Totals: count `Family` rows with `RegistrationDate >= date` (and `Orphans` created
        `CreatedOn >= date`) in scope; Details: page the same families ordered by
        `RegistrationDate DESC` through the `IUnitOfWork` repositories; orphans entered counted off
        the family's collection. Returns either the totals DTO or
        `ReportPagedResult<FamilyFollowUpDetailDto>` (18-1 envelope)
        **— `Task<object>` return with the mode switch in the service; explicit `!IsDeleted` at
        every level (this platform has no global soft-delete filter — the story's Dev Notes line
        was corrected, as in 18-12)**
- [x] **Task 3 — API endpoint** (AC 2, 6)
  - [x] `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — added
        `[HttpGet("{id}/follow-up")] GetFamilyFollowUp(Guid id, [FromQuery] …)` beside the
        literal `follow-up` action (UC-FAM-11's; different segment counts, no route ambiguity);
        `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`; `ValidationException` → 400
        `{ message, errors }`; catch-all → 500 `{ message }`. Raw envelope — no `ApiResponse<T>`
        (15-1 ruling)
- [x] **Task 4 — Frontend screen** (AC 1, 2, 5)
  - [x] `Frontend/src/app/modules/reports/family-orphans-entries/` — thin 4-file component in the
        18-1 shell; route `#/reports/family-orphans-entries` (reconciled — see Dev Agent Record),
        guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Date picker + charity drop-down (real endpoints only, `result.items || []`, كل الجهات
        all-option for HQ); the two data commands call
        `services/report.service.ts` → `GET /api/Families/{id}/follow-up` with `date`/`mode`;
        totals render as summary tiles, details in the bespoke grid + shared `Pagination` — NOT
        `data-list`; `trackBy`; empty state; no `OnPush` (list-screen precedent)
        **— the shell's search button IS تفاصيل; إجماليات is a second button in the filter panel;
        `showExport = false` (§23.S.10 has no استخراج)**
  - [x] The print button is NOT rendered (18-37 owns it)
- [x] **Task 5 — i18n** — title, field labels, command labels, totals tile labels, detail column
      headers, empty state under `reports.familyOrphans.*` in **both** `assets/i18n/ar.json` and
      `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live check: authenticated `mode=totals` → counts; `mode=details` → paged camelCase rows
        filtered by `date`; charity caller naming another charity's id → clamped to its own rows;
        HQ across charities works; future date → zeros/empty; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 = live-API output lock, never kill the
        user's process; ng-serve stale-bundle grep; Arabic payloads from UTF-8 files)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- This is the one EP-18 story whose endpoint lives on `FamiliesController`, not `ReportsController` —
  the board fixes `GET /api/Families/{id}/follow-up` and §23.U.13's realisation names
  `FamiliesController` → `IFamilyService`. Follow the existing child-route pattern
  (`[HttpGet("{familyId}/orphans")]`, line ~548); no new controller.
- `date` and `charityId` of the legacy payload are query/route inputs; caller identity still comes
  only from the JWT (`ICurrentUserService`) — a charity caller can never widen scope by naming
  another charity in the route.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controller inherits `ControllerBase` (17-1 note); camelCase wire; no
  `FK_*` keys; FluentValidation in the service; reads via `IUnitOfWork` repositories, repositories
  never save; global soft-delete filter — no manual `IsDeleted`; lookup labels `NameAr ?? NameEn`;
  bespoke grid + shared `Pagination`; no hardcoded lookup arrays.
- No entity, no migration — read-only projection over `Family`/`Orphan` and their audit stamps.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| طباعة الاستبانة / charity identification sheets (`PrintCharityIdentifications`) | 18-37 |
| Family update tracking print (`…/export/pdf`) | 18-21 |
| The `#/reports/family-orphans/export/pdf` legacy family list-by-date print | 18-39 |
| The generic display-in-browser / export-to-Excel engine | 18-40 / 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.10] screen contract — 2 fields,
  3 commands (print deferred)
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.13] scenario — what each charity
  entered on a given date
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-13 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs:548] the child-route pattern
  the new `follow-up` action copies
- [Source: Backend/src/IIROSA.Domain/Entities/Family.cs] `RegistrationDate`, `CharityId`,
  `Orphans` collection — the projection's source
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen reference
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed story reference:
  envelope, ControllerBase, grid + Pagination, build caveats

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Ground truth before smoke: `[IIROSA].[Family]` (live, `FK_CharityId` set) = **2**;
  orphans under them = **4**. Both families registered 2026-08-24.
- API smoke (temp instance, `127.0.0.1:60970`):
  anon `mode=totals` → **401**; HQ `mode=totals` no date → **200
  `{"newFamilies":2,"newOrphans":4,"sinceDate":null}`** (matches ground truth exactly);
  HQ `mode=details` → **200** paged rows (`totalCount:2`, both with orphansCount 2);
  `mode=bogus` → **400** (whitelist); future date `2030-01-01` → **200 zeros**
  (`newFamilies:0,newOrphans:0`); Charity-role claim-less caller (seed account) → **400
  "No charity is associated with this account"** — the fail-closed clamp inherited from
  UC-FAM-11's convention, never another charity's rows.
- **Smoke-caught defect and fix:** the first details response returned
  `charityName: null` for FAM-2026-3531. `Family` carries two charity columns — the legacy
  `CharityId` MIRROR (what the EF `Charity` navigation binds by convention) and the live
  `FK_CharityId` (post-transfer shape: mirror NULL, live set). The nav-based name projection
  therefore misses live-column-only rows. Fixed by projecting `FK_ChartyId` and resolving names
  through a dictionary over the page (the 18-x pattern); re-smoke shows both rows resolving
  (`dga`, `الجمعية الخيرية - وادى النطرون`). **Observed, not fixed here (5-11's screen, in
  review): `GetFollowUpActivityAsync` projects `f.Charity.Name` through the same nav and has the
  same miss for live-column-only rows — flagged to the 5-11 owner.**
- `dotnet build` → 0 errors (one MSB3021/3027 pass was MY leftover temp smoke instance holding
  the DLL, killed and rebuilt — the user's live API was never touched). `npm run build` →
  EXIT=0, `error TS` count = 0. i18n node validation: 17 `reports.familyOrphans.*` keys in both
  locales.

### Completion Notes List

- **Route reconciliation (the story's premise gap):** §23.S.10 and 5-11's §10.U.11 both fix
  `#/reports/family-orphans`, and 5-11's Dev Notes explicitly recorded "this story owns the route
  per spec §10.U.11 … 18-13's claim … must be reconciled there". This screen therefore takes the
  sibling route `#/reports/family-orphans-entries` — the same deviation class 18-39 already
  recorded for its print screen. **18-37 must target THIS component** (`family-orphans-entries`)
  when it adds the طباعة الاستبانة command; the downstream story files say "18-13's component"
  which is this one.
- One endpoint serves both data commands (`mode` switch in the service); the filter DTO carries
  the route id (`Guid.Empty` = كل الجهات, HQ only) + `date` + paging, and the validator whitelists
  the mode — the story's five-parameter service signature became the filter-DTO shape to match the
  platform's validator convention.
- Scope follows the HOST FILE's convention (role string from the controller +
  `GetUserCharityId()`), not `ICurrentUserService` — `GetFollowUpActivityAsync` precedent; a
  Charity-role caller without a charity claim fails closed with 400 rather than seeing anything.
- Totals semantics: families by `RegistrationDate >= date`, orphans by `CreatedOn >= date`
  (orphan set scoped through `o.Family.FK_CharityId` — the live column, consistent with the
  family set); date omitted → all entries. Details: same family set paged by
  `RegistrationDate DESC, Code`; عدد الأيتام is the family's live orphan count (one grouped
  query for the page, no N+1).
- §23.S.10 has no استخراج command — the shell renders without the export button
  (`showExport=false`); the shell's search button doubles as تفاصيل, إجماليات is a filter-panel
  button, and the print command is 18-37's, not rendered.
- Story Dev Notes' "global soft-delete filter — no manual IsDeleted" line was written before
  18-1's finding; this platform has no global filter — explicit `!IsDeleted` at family, orphan and
  charity levels (same correction recorded in 18-12).

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `FamilyEntryTrackingFilterDto`,
  `FamilyFollowUpTotalsDto`, `FamilyFollowUpDetailDto` (+ `CharityId` carry-along)
- `Backend/src/IIROSA.Application/Validators/Reports/FamilyEntryTrackingValidator.cs` — new,
  mode whitelist + page bounds
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` — `GetFamilyFollowUpAsync`
  declared; `using IIROSA.Application.DTOs.Reports` added
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — validator wired;
  `GetFamilyFollowUpAsync` (totals + details, scope clamp, live-column charity resolution,
  grouped orphan counts); `using IIROSA.Application.DTOs.Reports` added
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — `GET {id}/follow-up` action,
  `SuperAdmin,Admin,Charity`, anonymous-ladder errors; `using …DTOs.Reports` added
- `Frontend/src/app/modules/reports/models/report.model.ts` — `FamilyEntryTotals`,
  `FamilyEntryDetailRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getFamilyEntryTracking()`
  (GET + HttpParams, Guid.Empty for كل الجهات)
- `Frontend/src/app/modules/reports/family-orphans-entries/` — new 4-file component (date +
  charity filters, إجماليات tiles, تفاصيل 6-column grid, no export)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `family-orphans-entries` route,
  `Reports.View`
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 17
  `reports.familyOrphans.*` keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-13 and module spec §23.S.10 / §23.U.13; missing `follow-up` endpoint specified on `FamiliesController`, print command deferred to 18-37, two data commands scoped. |
| 2026-08-24 | Implemented. Route reconciled to `family-orphans-entries` (5-11 owns the legacy name per its recorded decision — 18-39 deviation class). Smoke-caught the EF-nav/charity-mirror null-name defect and fixed via live-column dictionary resolution; totals verified against sqlcmd ground truth (2/4); mode whitelist, future-date zeros, fail-closed charity clamp all verified; both builds green. 5-11's same-class defect flagged, not touched. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
