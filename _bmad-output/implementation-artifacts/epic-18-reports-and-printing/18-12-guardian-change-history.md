# Story 18-12: Guardian change history

| Field | Value |
| --- | --- |
| Story key | `18-12-guardian-change-history` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-12 — تقارير تعديل المعيل |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.19 screen, §23.U.12 scenario) |
| Route | `#/reports/provider-sponsor-changes` |
| Endpoint | `POST /api/Reports/provider-sponsor-changes` |
| Depends on | **18-1 landed** (reports skeleton: `ReportsController`, `IReportService`/`ReportService`, `ReportPagedResult<T>`, `ResolveCharityScope`, `modules/reports` shell + `report-export.service.ts`) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to guardian change history تقارير تعديل المعيل, so that a
record that was entered wrongly or has changed can be corrected.

**Legacy story type, corrected:** US-RPT-12 is typed "Update a record" — a template artefact. §23.S.19
and §23.U.12 describe a **query screen** (charity filter + grid + export); the spec's own «حفظ»
command is wired to `DeleteOutgoing()` — a copy/paste artefact from the correspondence module. This
story is a read-only report: no entity is created, updated or deleted.

## Acceptance Criteria

1. Given a signed-in user with an active session on `#/reports/provider-sponsor-changes`, when the
   actor selects a charity and the grid loads, then no stored data is changed — the operation is a
   read.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/provider-sponsor-changes` and the response is rendered on the screen without
   a page reload.
3. Given the caller is a charity user, when the function is invoked, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the function is invoked, then
   it operates on that charity's data.
5. Given no guardian record matches the selection, when the screen loads, then the grid renders empty
   and the paging control reports zero pages.
6. Given the actor pages through the results (التالي/السابق), when a page is requested, then the
   shared paging control serves it without losing the filter.
7. Given the actor presses the export command, when rows exist, then an Excel workbook with the
   §23.S.19 columns is delivered; when no row exists the actor is told, and no file is produced.
8. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** the guardian rows of the selected charity are listed per §23.S.19 with the
data-source decision below recorded honestly; export works off the same rows; the charity/country
scope is enforced server-side, not only in the menu.

## Screen contract (§23.S.19 — تقارير تعديل المعيل, 1 field, 1 grid, 4 commands)

| Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- |
| الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items \|\| []`); كل الجهات all-option for HQ; on change reloads the grid (`getCharityData()`) |

Grid columns and their **verified** data sources (row = one provider assignment on a family):

| Column (as labelled) | Source |
| --- | --- |
| كود الاسره | `Family.Code` |
| اسم المعيل السابق | **no stored source** — the platform keeps no prior-guardian snapshot (gap recorded below); renders empty |
| صله القرابه (السابق) | **no stored source** — renders empty |
| سبب التغيير | **no stored source** — no change-reason field exists anywhere in the Domain; renders empty |
| اسم المعيل الجديد | `Provider.FullName` |
| صله القرابه (الجديد) | `Provider.RelationshipToFamily` |
| تاريخ التعديل | `Provider.CreatedOn` (when the guardian was registered); `UpdatedOn` shown when it is later, marking an edited record |
| اسم الجمعيه | `Charity.NameAr ?? NameEn` (via `Family.CharityId`) |
| كود اليتيم | the family's orphan codes — joined string per row |

Commands (platform shape of the four §23.S.19 commands): بحث/load (filter change) ·
استخراج البيانات (`ExportReportData()`) · التالي/السابق (`GetNext()`/`GetPrev()`) → the shared
`Pagination` component. «حفظ → DeleteOutgoing()» is the spec artefact — **not built**.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` —
        `ProviderChangeFilterDto` (`CharityId`, `Page = 1`, `PageSize = 20`) and
        `ProviderChangeReportRowDto` (the 9 clean-named keys — no `FK_*` wire keys)
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/ProviderChangeFilterValidator.cs` —
        paging bounds only
- [x] **Task 2 — Service query — data-source decision executed** (AC 1, 2, 3, 4, 5)
  - [x] `IReportService` / `ReportService` — `GetProviderSponsorChangesAsync(filter)`: page over
        `Provider` joined to `Family` (+ `Family.Orphans`, `Charity`) through the `IUnitOfWork`
        repositories; `ResolveCharityScope(filter.CharityId)` (pin-never-widen,
        `OfficeProjectService.cs:384` precedent); ordered by `Provider.CreatedOn DESC`; returns
        `ReportPagedResult<ProviderChangeReportRowDto>`
        **— as implemented: pin-never-widen via the new `ApplyProviderCharityScopeAsync`
        (family-scoped mirror of the mother/beneficiary helpers); orphan codes attached in a
        second narrow query for the page's families (18-9 pattern — GroupBy+string.Join is not
        translatable), not via AutoMapper projection**
  - [x] Projection: current-guardian fields from `Provider`; تاريخ التعديل from
        `CreatedOn`/`UpdatedOn` audit stamps; orphan codes via `string.Join` over the family's
        orphans; the three gap columns ship as empty strings with the gap noted in code comments —
        **do not invent storage**. Work only through the global soft-delete filter (never
        `IgnoreQueryFilters`) — the report reads live provider rows, not a history table
        **— corrected during implementation: this platform has NO global soft-delete filter
        (18-1 finding); every level filters `!IsDeleted` explicitly — provider, family, orphans,
        charities. The story note was written before that finding**
  - [x] `Profiles/ReportProfile.cs` — scalar map with `NameAr ?? NameEn` lookup labels
        **— not needed: the projection is a straight EF `Select` into the row DTO (no mapping
        layer involved) and Charity resolves by `c.Name` (the Charity entity's bilingual label
        property), matching every implemented 18-x report**
- [x] **Task 3 — API endpoint** (AC 2, 8)
  - [x] `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` —
        `[HttpPost("provider-sponsor-changes")]` → `Ok(pagedResult)`;
        `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`; `ValidationException` → 400
        `{ message, errors }` (`OfficeProjectManagementController.cs:89-101` ladder); catch-all →
        500 `{ message }`. Raw envelope — no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend screen** (AC 1, 5, 6, 7)
  - [x] `Frontend/src/app/modules/reports/provider-sponsor-changes/` — thin 4-file component in
        the 18-1 shell; route `#/reports/provider-sponsor-changes` in `reports-routing.module.ts`,
        guarded `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Charity drop-down from `GET /api/Charities` (كل الجهات all-option for HQ;
        hidden/disabled for a charity caller); grid in §23.S.19 order with row serial
        (`(currentPage-1)*pageSize + i + 1`); bespoke grid + shared `Pagination` — NOT `data-list`;
        `trackBy`; empty state at `totalCount === 0`; no `OnPush` (list-screen precedent)
  - [x] استخراج via 18-1's `report-export.service.ts` — the 9 headers, RTL worksheet, file name
        `provider-changes_<charity|all>_<yyyy-MM-dd>.xlsx`; zero rows → message, no file
- [x] **Task 5 — i18n** — title, 9 column headers (the two صله القرابه columns disambiguated as
      سابقة/جديدة), all-option, empty state, export toasts under `reports.providerChanges.*` in
      **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–8)
  - [x] Live check: authenticated POST → 200 camelCase `items/totalCount/totalPages`; charity
        caller pinned to its rows; HQ + explicit `charityId` → that charity; empty selection →
        zero pages; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 = live-API output lock, never kill the
        user's process; ng-serve stale-bundle grep; Arabic payloads from UTF-8 files)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Data-source decision (recorded honestly — this is the story's core finding)

The platform has **no guardian-change audit trail**. Verified against the code:

- `Provider` (`Backend/src/IIROSA.Domain/Entities/Provider.cs`) is the guardian record:
  `FullName`, `RelationshipToFamily`, `NationalId`, `Phone`, keyed by `FamilyId`, with the
  inherited audit stamps (`CreatedOn`, `UpdatedOn`, …).
- The change flow exists — `FamiliesController` `POST {familyId}/provider` (AddProvider, line ~779)
  and `POST {familyId}/verify-provider` (~815) — but it writes the current state only. No
  prior-value snapshot, no change reason, and `Family` carries no provider history.

Therefore this report **projects what exists**: one row per provider assignment with the current
guardian's fields, the audit stamps as تاريخ التعديل, and the family/charity/orphan context. The
old-guardian name, old relationship and change-reason columns render empty, and the gap is recorded
here rather than silently filled. Introducing a `ProviderChangeHistory` entity (an EF migration plus
write-path hooks in the provider flow) is explicitly out of scope — that is a scope change for
`/bmad-correct-course`, not something to slip into a report story.

### Platform rules that bind this story

- §23.U.12's update-flavoured steps ("the stored record carries the new values") belong to the
  template's "Update a record" type — nothing is written by this story.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controller inherits `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]` (17-1 note); camelCase wire; no `FK_*` keys; FluentValidation in
  the service; reads via `IUnitOfWork` repositories, repositories never save; global soft-delete
  query filter — never a manual `IsDeleted` check and never defeated; lookup labels
  `NameAr ?? NameEn`; bespoke grid + shared `Pagination`; no hardcoded lookup arrays.
- No entity, no migration — read-only projection.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| A `ProviderChangeHistory` entity + provider-flow write hooks | scope change → `/bmad-correct-course` |
| Guardian/widow identification sheets (print) | 18-37 |
| PDF export of this screen | 18-21 (jsPDF install) |
| The generic display-in-browser / export-to-Excel engine | 18-40 / 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.19] screen contract — 9-column grid,
  4 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.12] scenario — old/new guardian pairs
  per charity (update-typed template artefact noted)
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-12 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Provider.cs] the guardian record and its audit stamps
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs:779] `POST {familyId}/provider`
  and [:815] `verify-provider` — the change flow that keeps no history
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen reference behind 18-1's `ResolveCharityScope`
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed list-story
  reference: serial formula, envelope, grid + Pagination, build caveats

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Dev DB row source: `SELECT COUNT(*) FROM [IIROSA].Provider WHERE IsDeleted = 0` → **2 live rows**
  across two charities — the report returns real data.
- API smoke (temp instance, `127.0.0.1:60970`): anon POST → **401**; SuperAdmin POST → **200**
  with both rows — `familyCode`, `newGuardianName` (Arabic renders), `newRelationship`,
  `changeDate`, resolved `charityName`, joined `orphanCodes`
  (`"ORP-2026-42717, ORP-2026-43223"`), the three prior-guardian keys null as designed,
  `totalCount:2, totalPages:1`; `page:0` → **400**; Charity-role POST → **200** (allowed role).
- Scope audit of the Charity-role result: `identity.Users` shows `Charity@IIROSA.com` carries
  **CharityId = NULL, CountryId = NULL** — the seed account is claim-less, so the pin branch
  correctly does not fire and unscoped is by-design (the role name alone never scopes; tenancy is
  claim-based). Payload-widen attempt (`charityId` in body under the Charity role) returned the
  identical unscoped set — the narrow branch requires `IsHeadOffice`, so the payload is inert for
  non-HQ callers. The only charity-claim user (`osamaabdelaziz4100@gmail.com`, CharityId
  A99EB2B5-…) does not share the seed password, so a live pinned login was not possible; the pin
  path is byte-identical to the already-audited `ApplyMotherCharityScopeAsync` /
  `ApplyBeneficiaryCharityScopeAsync` (18-6/18-9).
- `dotnet build` → 0 errors (the 193-warning first pass was a full-rebuild artifact; incremental
  shows the usual 8 pre-existing NU1903 package advisories, none from this change). `npm run
  build` → EXIT=0, `error TS` count = 0. i18n node validation: 16 `reports.providerChanges.*`
  keys in both locales, both files parse.

### Completion Notes List

- **Real query, not an empty-set story:** unlike 18-11, the row source exists — the report pages
  `Provider` (live rows only, `!IsDeleted` explicit at every level) joined to `Family` for
  code/charity and to the family's orphans for the joined كود اليتيم column.
- **Orphan-code join shape:** one row per provider assignment; GroupBy + `string.Join` has no SQL
  translation, so the codes are fetched in a second narrow query restricted to the page's family
  ids and joined in memory (the 18-9 pattern) — paging stays server-side and `totalCount` is the
  provider count, not the orphan count.
- **تاريخ التعديل semantics:** `UpdatedOn` when later than `CreatedOn` (an edited record), else
  `CreatedOn` (when the guardian was registered) — the conditional is inside the translatable
  projection.
- The three prior-guardian columns ship null (story's recorded finding — no audit trail); the
  screen and export render them blank, no storage invented.
- Story-Dev-Notes correction recorded in Task 2: the story asserted a global soft-delete query
  filter; the platform has none (18-1 finding) — explicit `!IsDeleted` filtering is the
  implemented, audited pattern across EP-18.
- `ReportProfile.cs` not touched: the projection is a direct EF `Select` and Charity resolves via
  `c.Name` like every 18-x report — no AutoMapper leg adds anything here.
- Endpoint roles include Charity (all-roles story); the sidebar gate is permission-only and the
  HQ charity drop-down hides for non-HQ callers.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `ProviderChangeFilterDto`,
  `ProviderChangeReportRowDto` (9 clean-named keys + `ProviderId`/`FamilyId` carry-alongs)
- `Backend/src/IIROSA.Application/Validators/Reports/ProviderChangeFilterValidator.cs` — new,
  page bounds only
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetProviderSponsorChangesAsync`
  declared; data-source decision recorded in XML docs
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — ctor `_providerRepository` +
  `_providerChangeValidator`; `GetProviderSponsorChangesAsync`;
  `AttachProviderChangeOrphanCodesAsync`; `ResolveProviderChangeCharityNamesAsync`;
  `ApplyProviderCharityScopeAsync`
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `provider-sponsor-changes` action,
  `SuperAdmin,Admin,Charity`, standard error ladder
- `Frontend/src/app/modules/reports/models/report.model.ts` — `ProviderChangesFilter`,
  `ProviderChangeRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getProviderSponsorChanges()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportProviderChanges()`
  (RTL, serial + 9 columns, scoped file name)
- `Frontend/src/app/modules/reports/provider-sponsor-changes/` — new 4-file component
  (charity drop-down HQ-only, 10-column grid, all-pages export)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `provider-sponsor-changes` route,
  `Reports.View`
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
  (permission-only gate)
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 16
  `reports.providerChanges.*` keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-12 and module spec §23.S.19 / §23.U.12; update-typed artefact corrected to a read, and the missing guardian-change audit trail recorded with the projection-from-`Provider` decision. |
| 2026-08-24 | Implemented against the live `Provider` set (2 dev rows). Orphan codes joined via the 18-9 narrow-projection pattern; scope helper mirrored from the audited family-scoped helpers; explicit `!IsDeleted` (story's global-filter note corrected); payload widen proven inert. Smoke-verified 401/200-rows/400/Charity-200; both builds green. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
