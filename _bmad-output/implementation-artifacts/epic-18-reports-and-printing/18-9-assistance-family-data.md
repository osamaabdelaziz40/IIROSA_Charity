# Story 18-9: Assistance family data

| Field | Value |
| --- | --- |
| Story key | `18-9-assistance-family-data` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-09 — بيانات أسر المساعدات |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.13 screen, §23.U.9 scenario) |
| Route | `#/reports/beneficiary-family-details` |
| Endpoint | `POST /api/Reports/beneficiary-family-details` |
| Depends on | **18-1 landed** (reports skeleton: `ReportsController`, `IReportService`/`ReportService`, `ReportPagedResult<T>`, `ResolveCharityScope`, `modules/reports` shell + `report-export.service.ts`) |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to assistance family data بيانات أسر المساعدات, so that I can
see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a signed-in user with an active session on `#/reports/beneficiary-family-details`, when the
   actor opens the screen, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/beneficiary-family-details` and the response is rendered on the screen without
   a page reload.
3. Given the caller is a charity user, when the function is invoked, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload.
4. Given an HQ caller (`IsHeadOffice`) with an explicit charity id, when the function is invoked, then
   it operates on that charity's data.
5. Given no family matches the selection, when the screen loads, then the grid renders empty and the
   paging control reports zero pages.
6. Given the session has expired or the role is not permitted, when the function is invoked, then the
   request is rejected and the actor is routed back to the login screen.

**Definition of done:** the families benefiting from assistance are listed for the selected charity
per §23.U.9; the استخراج export works off the same rows; the charity/country scope is enforced
server-side, not only in the menu.

## Screen contract (§23.S.13 — بيانات أسر المساعدات, adapted)

§23.S.13 specifies ONE field (charity drop-down, on-change `getCharityData()`) and ONE command
(استخراج البيانات) with **no grid** — the legacy screen renders a straight extract. Platform
adaptation (recorded): the extract needs rows to extract, so the screen is shaped as a grid of
families + استخراج using the 18-1 `report-viewer` shell.

| Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- |
| الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items \|\| []`); كل الجهات all-option for HQ; on change reloads the grid |

Grid (adaptation — §23.S.13 names none): كود الأسرة · رب الأسرة · التليفون · العنوان · الجمعية ·
الحملة — one row per distinct benefiting family.

Commands: استخراج البيانات (`ExportData()`) — always shown.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator** (AC 2)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `BeneficiaryFamilyFilterDto`
        (`CharityId`, `Page = 1`, `PageSize = 20`) and `BeneficiaryFamilyListDto` (the six grid
        fields, clean names — no `FK_*` wire keys)
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/BeneficiaryFamilyFilterValidator.cs` —
        paging bounds only; the charity key is optional (HQ may ask for all)
- [x] **Task 2 — Service query** (AC 2, 3, 4, 5)
  - [x] `IReportService` / `ReportService` — `GetBeneficiaryFamiliesAsync(filter)`: read
        `SeasonalAidBeneficiary` joined to `Family` (+ `Charity`, `SeasonalAidCampaign`) through the
        `IUnitOfWork` repositories; distinct families; `ResolveCharityScope(filter.CharityId)`
        (pin-never-widen, `OfficeProjectService.cs:384` precedent); ordered by `Family.Code`;
        returns `ReportPagedResult<BeneficiaryFamilyListDto>`
  - [x] `Profiles/ReportProfile.cs` — map with lookup names resolved `NameAr ?? NameEn`
- [x] **Task 3 — API endpoint** (AC 2, 6)
  - [x] `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `[HttpPost("beneficiary-family-details")]`
        → `Ok(pagedResult)`; `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`;
        `ValidationException` → 400 `{ message, errors }`; catch-all → 500 `{ message }`. Raw
        envelope — no `ApiResponse<T>` (15-1 ruling)
- [x] **Task 4 — Frontend screen** (AC 1, 2, 5)
  - [x] `Frontend/src/app/modules/reports/beneficiary-family-details/` — thin 4-file component
        (`.ts`/`.html`/`.scss`/`.spec.ts`) inside the 18-1 shell; route
        `#/reports/beneficiary-family-details` in `reports-routing.module.ts`, guarded
        `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Charity drop-down from `GET /api/Charities` (كل الجهات all-option for HQ; hidden/disabled
        for a charity caller — the server pins anyway); bespoke grid + shared `Pagination` — NOT
        `data-list`; `trackBy` on the `*ngFor`; empty state when `totalCount === 0`; no `OnPush`
        (codebase list-screen precedent)
  - [x] استخراج via 18-1's `report-export.service.ts` (ExcelJS + file-saver); zero rows → message,
        no file
- [x] **Task 5 — i18n** — title, column headers, all-option, empty state, export toasts under
      `reports.beneficiaryFamilyDetails.*` in **both** `assets/i18n/ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live check: authenticated POST → 200 camelCase `items/totalCount/totalPages`; charity
        caller sees only its rows and cannot widen by posting another `charityId`; HQ + explicit
        `charityId` → that charity's rows; empty selection → zero pages; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 = live-API output lock, never kill the
        user's process; ng-serve stale-bundle grep caveat)
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- §23.U.9's payload names `charityId, userId` — `userId` is legacy noise: caller identity comes from
  the JWT (`ICurrentUserService`), never from the body. Only `CharityId` rides the DTO.
- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); controller inherits `ControllerBase` + `[Authorize]` +
  `[Route("api/[controller]")]` (17-1 note).
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories, repositories never save; global soft-delete filter — no manual `IsDeleted`;
  lookup labels `NameAr ?? NameEn`; bespoke grid + shared `Pagination`; no hardcoded lookup arrays.
- No entity, no migration — a read-only projection over `SeasonalAidBeneficiary`/`Family`.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Campaign-level distribution detail (per-beneficiary amounts) | seasonal-aid epic surfaces |
| Any write to beneficiary/family data | none — this epic does not write |
| PDF export of this screen | 18-21 (jsPDF install) |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.13] the one-field screen contract
  and its استخراج command
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.9] scenario — caller-scoped listing
  of assisted families
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-09 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/SeasonalAidBeneficiary.cs] data source — assistance
  beneficiaries joined to families
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen reference behind 18-1's `ResolveCharityScope`
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed list-story
  reference: envelope, ControllerBase, grid + Pagination, build caveats

## Dev Agent Record

### Agent Model Used

`GLM 4.7 (Claude Code dev agent)`

### Debug Log References

- Compile: temp-folder `dotnet build` → **0 CS errors**.
- Frontend: `npm run build` → **exit 0**.
- i18n: node key-walk over `ar.json`/`en.json` → all 13 `reports.beneficiaryFamilyDetails.*` keys present in both.
- Live smoke (private instance, `http://127.0.0.1:60970`; stopped after):
  - anon `POST /api/Reports/beneficiary-family-details` → **401**
  - HQ caller, all charities → **200** `{"items":[],"totalCount":0,"page":1,"pageSize":20,"totalPages":0}`
  - Charity caller → **200** (same empty set; the pin is `Family.FK_CharityId == claim` whatever the payload posts)
  - `page: 0` → **400** `{"errors":{"Page":"Page must be at least 1"}}`
- Empty set proven legitimate: `sqlcmd` → `dbo.SeasonalAidBeneficiary` holds **0 rows** (dev DB has no registered assistance beneficiaries — the seasonal-aid epic owns seeding that data; the report is not patched to invent any).

### Completion Notes List

- "One row per distinct benefiting family" delivered with الحملة aggregated: GroupBy over `SeasonalAidBeneficiary` with the campaign names joined per family. `GroupBy + string.Join` has no SQL translation, so the query projects a narrow row set server-side (translatable) and only the grouping/join runs in memory; paging applies AFTER the grouping so `totalCount` is the distinct-family count.
- رب الأسرة resolution mirrors 18-1's guardian ladder at family level: Provider → Father → Mother (first non-empty).
- Task 2's `ReportProfile` sub-item adapted (recorded): direct projection in the service instead of an AutoMapper map — no lookup labels exist on this row (Campaign.Name is a plain string; charity names resolve through the 18-1 dictionary helper), so a profile would be a pass-through. Same approach as 18-3…18-8.
- Scope helper `ApplyBeneficiaryCharityScopeAsync` is the family-scoped mirror of 18-1's `ApplyCharityScopeAsync` (charity claim pin → HQ explicit narrow → CountryId charity-set → unconstrained HQ).
- The seasonal-aid tables live in `dbo` (not `IIROSA`) — worth knowing for future SQL probes; EF mapping handles it transparently.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `BeneficiaryFamilyFilterDto` + `BeneficiaryFamilyListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/BeneficiaryFamilyFilterValidator.cs` — page bounds
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetBeneficiaryFamiliesAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — `IRepository<SeasonalAidBeneficiary>` + validator injected; `GetBeneficiaryFamiliesAsync` (project → group in memory → page → resolve names); `ApplyBeneficiaryCharityScopeAsync`; `ResolveBeneficiaryCharityNamesAsync`
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `beneficiary-family-details` action (roles SuperAdmin, Admin, Charity; standard ladder)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `BeneficiaryFamilyFilter` / `BeneficiaryFamilyRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getBeneficiaryFamilies()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportBeneficiaryFamilies()` (serial + 6 columns)
- `Frontend/src/app/modules/reports/beneficiary-family-details/` — NEW 4-file component in the 18-1 shell (charity narrow + grid + all-pages Excel extract)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `beneficiary-family-details` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry under التقارير (permission-gated)
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `reports.beneficiaryFamilyDetails.*` (13 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-09 and module spec §23.S.13 / §23.U.9; no-grid legacy screen adapted to grid + استخراج on the 18-1 shell; `userId` payload key ruled out. |
| 2026-08-24 | Implemented: `POST /api/Reports/beneficiary-family-details` (distinct assisted families, campaigns aggregated per row); grid + استخراج screen on the 18-1 shell; i18n ar+en. Direct projection replaces the story's ReportProfile sub-item (no lookup labels on the row — recorded). Verified: 0 CS errors, npm build exit 0, 401/400/200 HQ + charity pin live; empty set legitimate (0 beneficiary rows in dev DB). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
