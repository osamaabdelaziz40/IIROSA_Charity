# Story 18-1: Orphan data بيانات الأيتام

| Field | Value |
| --- | --- |
| Story key | `18-1-orphan-data` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-01 — بيانات الأيتام |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.3 screen, §23.U.1 scenario) |
| Route | `#/reports/orphans` |
| Endpoint | `POST /api/Reports/orphans` |
| Depends on | EP-01 (authentication and role resolution) — nothing in this epic precedes it |
| Roles | All roles → `SuperAdmin`, `Admin`, `Charity` (`Reports.View`) |

## Status

done

## Story

As a signed-in user, I want to be able to orphan data بيانات الأيتام, so that I can see the full
detail of a single record before acting on it.

## Acceptance Criteria

1. Given a signed-in user with an active session on the screen at `#/reports/orphans`, when the
   actor presses «بحث», then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/orphans` with a typed request DTO and the raw paged envelope
   (`items/totalCount/page/pageSize/totalPages`, camelCase) is rendered on the screen without a
   page reload.
3. Given the caller is a charity user, when the report runs, then only that charity's rows are
   returned — the scope is pinned server-side from `ICurrentUserService.CharityId`, never from the
   payload. Given an HQ caller (`IsHeadOffice`), when an explicit charity id is supplied, then the
   report runs on that charity's data. A caller with a `CountryId` claim is additionally pinned to
   the charities of that country — pin-never-widen (`OfficeProjectService.cs:384` shape).
4. Given no row matches the filters, when the report runs, then the grid renders empty and the
   paging control reports zero pages.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected with 401/403 and the actor is routed back to the login screen.
6. Given `AgeTo` is lower than `AgeFrom`, or either age is below 1, when «بحث» is pressed, then
   the request is refused with a `message` and the report does not run (the epic's «Faliure» rule,
   tightened to the only failure this read can produce).

**Definition of done:** the 10 filter fields of §23.S.3 are implemented with their lookups and the
mutual-exclusion checkbox handlers; the orphan grid renders its column subset with horizontal
scroll; استخراج البيانات delivers an Excel workbook of the full column set; the scenario of
§23.U.1 passes end to end; the role and charity scoping is enforced server-side, not only in the
menu.

## Reality check: this is a GREENFIELD module — the report catalogue starts here

**No Reports code exists anywhere** (verified: no `ReportsController`, no `IReportService`, no
`reports` Angular module under `Frontend/src/app/modules/`, no `DashboardController`). This story
builds the **epic skeleton** every later 18-x story adds a report key to.

**Epic-wide ruling (binding):** EP-18 introduces **no new entities and no EF migration**. Reports
are read-only projections over existing entities — `Orphan` (+ `Family`, `Father`, `Mother`,
`Provider` navs), `OrphanPayment`, `PeriodicOrphanReport`. Where a legacy report column has no
backing column today (e.g. exclusion reason, Meza card), the DTO carries the key and the column
renders empty; the gap is recorded, never patched with a migration in this epic.

## What this story lands (the epic skeleton — later stories MUST reuse, not rebuild)

| Piece | Where | Notes |
| --- | --- | --- |
| `ReportsController` | `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` | `[Route("api/[controller]")]`, inherits `ControllerBase`, `[Authorize]` per action |
| `IReportService` / `ReportService` | `IIROSA.Application/Interfaces` / `Services` | one method per report key |
| `ResolveCharityScope(Guid? charityId)` | `ReportService` (private) | charity user pinned to `ICurrentUserService.CharityId`; `IsHeadOffice` may pass explicit `charityId`; `CountryId` claim additionally pins charities of that country — pin-never-widen |
| `ReportPagedResult<T>` | `DTOs/Reports/Reports.cs` | `Items`, `TotalCount`, `Page`, `PageSize`, `TotalPages` — OfficeProject shape (`OfficeProjects.cs:187`) |
| Validators | `Validators/Reports/` | FluentValidation, invoked in the service layer |
| Profile | `Profiles/ReportProfile.cs` | lookup names resolve `NameAr ?? NameEn` |
| DI | `Infrastructure/Extensions/ServiceCollectionExtensions.cs` | beside the mission lines (~163) |
| Frontend module | `Frontend/src/app/modules/reports/` | lazy; `reports-routing.module.ts` (one route per report key), shared `report-viewer/` shell, per-report thin 4-file components `<key>-report/` |
| `services/report.service.ts` | same module | one POST per report key |
| `services/report-export.service.ts` | same module | ExcelJS + file-saver (both already in `package.json`); shared by every report |
| `models/report.models.ts` | same module | filter/list/envelope types |
| Permissions | `auth.service.ts` `PERMISSION_ROLES` (~line 60) | `Reports.View` → `['SuperAdmin','Admin','Charity']` |
| Sidebar | `layouts/main-layout` | menu التقارير gated by `hasPermission('Reports.View')` |
| i18n | `assets/i18n/ar.json` + `en.json` | `reports.*` namespace |

## Screen contract (§23.S.3 — بيانات الأيتام, 10 filter fields, 9 commands)

| Label | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- |
| الدفعة المالية المنصرقة للايتام | `BatchNumber` | Drop-down | No | lookup Batches + كل الدفعات — `GET /api/OrphanPayments/batch-numbers` (`OrphanPaymentsController.cs:68`) |
| الجمعية | `CharityId` | Drop-down | No | lookup Charities + كافة الجهات — `GET /api/Charities` (`result.items || []`); HQ only — hidden for charity users |
| المركز | `CenterId` | Drop-down | No | lookup Centers + كل — `GET /api/LookupManagement/centers/by-region/{regionId}`; disabled until a governorate is chosen |
| المحافظة | `GovernorateId` | Drop-down | No | lookup Regions + كل — `GET /api/LookupManagement/regions`; on change reloads centers |
| من | `AgeFrom` | Numeric box | No | min 1 |
| الى | `AgeTo` | Numeric box | No | min 1; ≥ `AgeFrom` |
| العمر | `IsFinishedSponsorship` | Check box | No | legacy binds the العمر label to this flag — spec artefact, kept as the finished-sponsorship switch (feeds 18-4 semantics) |
| (unlabelled) | `NotExcluded` | Check box | No | default checked |
| المستبعدين | `Excluded` | Check box | No | on change → `DisableAllOrphan()` |
| (unlabelled) | `AllOrphans` | Check box | No | on change → `DisableExcluded()` — mutually exclusive with `Excluded` |

Commands (9 in §23.S.3 — **only the first and last are in scope here**):

| Command | Handler | In scope |
| --- | --- | --- |
| بحث | `GetData()` | **Yes** |
| بيانات الأيتام وأسرهم المطلوب كفالتهم | `GetOrphanThatRequiredWarranty()` | No — later maintenance story |
| كروت التسليم | `PrintRecieveCards()` | No — 18-31 |
| بيانات الأيتام - معيل آخر | `GetOrphanOtherSponser()` | No — later maintenance story |
| الأيتام المطلوب إرسال تقاريرهم للهيئة | `NewActionForWaleed()` | No — later maintenance story |
| متابعة الايتام - لم يتم الصرف | `GetRecievedPaymentDetails()` | No — 18-28 |
| تقرير بيانات غير المستلمين | `GetGotItNotPaymentDetails()` | No — later maintenance story |
| أيتام لم تصل لهم أي مبالغ | `OrphnasDontTakeAnyAmount()` | No — 18-28 family |
| استخراج البيانات | `ExportData()` | **Yes** |

## Grid (§23.S.3 — 44 columns, grouped)

Column groups: **identity** (رقم اليتيم، أسم اليتيم، الرقم القومى، تاريخ الميلاد، العمر،
النوع) · **guardian** (أسم المعيل، صلة القرابة، الرقم القومى للمعيل، مؤهل المعيل، مهنة
المعيل، الحالة الصحية للمعيل، الحالة الإجتماعية للمعيل، مشروع تنموى للمعيل) · **residence
& income** (المحافظة، المركز، القرية/الحى، العنوان التفصيلى، ت الموبايل، ت الموبايل2،
ملكية السكن، قيمة الايجار، نوع السكن، حالة مستويات السكن، قيمة الدخل) · **status**
(الحالة الاجتماعية، الحالة الصحية، نوعية العمل) · **education** (المرحلة الدراسية، الصف
الدراسى، اسم الموسسة التعليمية، الكلية، القسم، حاصل على موهل دراسى) · **father**
(تاريخ وفاة الاب، سبب وفاة الاب) · **exclusion** (الاستبعاد، سبب الاستبعاد) · **audit/org**
(الملاحظات، أسم الجمعية، تاريخ اخر تحديث).

**Recorded decision:** the on-screen grid shows a sensible subset (~24 columns: identity,
guardian, residence, status, education, exclusion, org) with horizontal scroll; the Excel export
carries the full 44-column contract. Columns without a backing field on `Orphan`/`Family`/
`Father`/`Mother`/`Provider` today render blank — do not invent data, do not migrate.

## Tasks / Subtasks

- [x] **Task 1 — Application skeleton + orphan-data query** (AC 2, 3, 6)
  - [x] `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs`: `OrphanDataFilterDto`
        (`Page = 1`, `PageSize = 20`, `BatchNumber`, `CharityId`, `GovernorateId`, `CenterId`,
        `AgeFrom`, `AgeTo`, `AllOrphans`, `Excluded`, `NotExcluded` — clean names, never `FK_*`),
        `OrphanDataListDto` (the 44 grid keys with resolved lookup names), `ReportPagedResult<T>`
        (OfficeProject shape)
  - [x] `Interfaces/IReportService.cs` + `Services/ReportService.cs`:
        `GetOrphanDataAsync(OrphanDataFilterDto)` → `ResolveCharityScope(filter.CharityId)` first,
        then filter via the `IUnitOfWork` orphan repository (includes `Family`, `Father`,
        `Mother`, `Provider`), age from `DateOfBirth`, ordered by `Code`; flag combination:
        `AllOrphans` = no exclusion predicate, `Excluded` = excluded set only, default =
        not-excluded — repositories never save, nothing writes
  - [x] `Profiles/ReportProfile.cs` — projection map; lookup labels `NameAr ?? NameEn`
  - [x] `Validators/Reports/OrphanDataFilterValidator.cs`: `AgeFrom/AgeTo ≥ 1`,
        `AgeTo ≥ AgeFrom` when both set, `PageSize ≤ 200`; invoked with `ValidateAndThrowAsync`
        in the service
- [x] **Task 2 — API endpoint** (AC 2, 5)
  - [x] `Backend/src/IIROSA.Api/Controllers/ReportsController.cs`: inherits `ControllerBase`,
        `[Route("api/[controller]")]`; `[HttpPost("orphans")] [Authorize(Roles =
        "SuperAdmin,Admin,Charity")]` → `Ok(pagedResult)`; catch
        `FluentValidation.ValidationException` FIRST → 400 `{ message, errors }`
        (`OfficeProjectManagementController.cs:89-101` ladder); catch-all → 500 `{ message }`
  - [x] **Do NOT wrap in `ApiResponse<T>`** — raw envelope + anonymous `{ message }` errors
        (15-1 ruling, architecture.md §10; zero live controllers use ApiResponse)
- [x] **Task 3 — DI** — register `IReportService` + `ReportService` in
      `Infrastructure/Extensions/ServiceCollectionExtensions.cs` beside the mission lines (~163)
- [x] **Task 4 — Frontend module skeleton** (AC 1, 2, 4)
  - [x] `Frontend/src/app/modules/reports/reports.module.ts` (lazy, NgModule style),
        `reports-routing.module.ts` with the `#/reports/orphans` route,
        `canActivate: [AuthGuard, PermissionGuard]`, `data: { permission: 'Reports.View' }`
  - [x] `report-viewer/` 4-file shared shell: filter panel slot + bespoke grid + shared
        `Pagination` + استخراج البيانات command — **do NOT use `data-list`** (recorded
        deviation); list screens omit `OnPush` (codebase precedent); `trackBy` on the `*ngFor`
  - [x] `orphan-data-report/` thin 4-file component feeding the shell
  - [x] `services/report.service.ts` (`getOrphanData(filter)`), `services/report-export.service.ts`
        (ExcelJS + `file-saver`, both already in `package.json` — verified),
        `models/report.models.ts`
  - [x] Register `loadChildren` in `Frontend/src/app/app-routing.module.ts` beside hq-transfers
- [x] **Task 5 — Filter panel** (AC 6) — batches → `GET /api/OrphanPayments/batch-numbers`;
      charities → `GET /api/Charities` with the كل/كافة الجهات "all" option for HQ only;
      governorates → `GET /api/LookupManagement/regions`; centers →
      `GET /api/LookupManagement/centers/by-region/{regionId}` reloaded on governorate change;
      age numerics reject non-numeric; the three orphan-flag checkboxes with the
      `DisableAllOrphan()` / `DisableExcluded()` mutual-exclusion handlers. No hardcoded option
      arrays — fixed vocabularies go through i18n keys
- [x] **Task 6 — Grid** (AC 2, 4) — serial column `(currentPage-1)*pageSize + i + 1` (13-1
      formula); ~24-column subset with horizontal scroll per the recorded decision; empty state
      when `totalCount === 0`
- [x] **Task 7 — Excel export** (AC 1) — استخراج البيانات → `report-export.service.ts` builds
      the full 44-column workbook client-side from the current filter (all pages, `PageSize` cap);
      when `totalCount === 0`, show the nothing-to-produce message instead of an empty file
- [x] **Task 8 — Permissions, sidebar, i18n** (AC 5) — `Reports.View` →
      `['SuperAdmin','Admin','Charity']` in `auth.service.ts` `PERMISSION_ROLES`; sidebar menu
      التقارير in `layouts/main-layout` gated by `hasPermission`; `reports.*` block (menu, title,
      all filter labels, all grid headers, empty state, export toasts) in **both** `ar.json` and
      `en.json`
- [x] **Task 9 — Verification** (AC 1–6)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 on copy steps = the user's
        live API locking outputs; compile is clean; never kill their process)
  - [x] Live check: anonymous POST → 401; charity token → only own rows; SuperAdmin → 200
        camelCase `items/totalCount/totalPages`; `ageTo < ageFrom` → 400 `{message}`; Arabic
        payloads sent from a UTF-8 file in curl checks (inline bodies show `?????` from the
        Git-Bash codepage — an artifact)
  - [x] `cd Frontend && npm run build` — 0 errors; ng-serve stale-bundle caveat: grep the served
        chunk for a new key before trusting a no-effect fix
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10). Controllers inherit `ControllerBase` with `[Authorize]`/`[Route]` — zero
  live controllers use a custom base (17-1 note).
- No `FK_*` DTO property names — Newtonsoft emits `fK_…` (13-3/15-6 defect class); camelCase wire.
- FluentValidation in the service layer; reads via `IUnitOfWork` repositories, which never save.
- Soft delete handled by the global query filter — never a manual `IsDeleted` check.
- Lookup labels `NameAr ?? NameEn`; bespoke grid + shared `Pagination` (NOT `data-list`);
  lazy module; list screens omit `OnPush` (codebase precedent).
- Caller scope from `ICurrentUserService` in the service — never parse claims in the controller,
  never trust the payload's `charityId` for a charity caller.
- PDF/Excel decision: client-side only — Excel via ExcelJS here; PDF via jsPDF is NOT in
  `package.json` and is not this story's concern (the shell's export command stays Excel-only).
  Legacy server `…/export/pdf` endpoints and EPPlus are superseded on this platform (recorded
  deviation).

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Excluded-orphans screen `#/reports/excluded-orphans` | 18-3 |
| Orphan-status refined query (periodic reports module) | 18-2 |
| Finished-sponsorship orphans screen | 18-4 |
| Unsponsored orphans screen | 18-5 |
| Widows-allowing-sponsorship screen | 18-6 |
| Meza-cards screen | 18-7 |
| Photo export صور الأيتام | 18-24 |
| Certificate export صور الشهادات | 18-25 |
| كروت التسليم receipt cards command | 18-31 |
| متابعة الايتام - لم يتم الصرف / أيتام لم تصل لهم أي مبالغ | 18-28 |
| بيانات الأيتام وأسرهم المطلوب كفالتهم (warranty variant), معيل آخر (other-sponsor variant), reports-to-authority command | later maintenance stories |
| jsPDF / server-side PDF pipeline | not in this batch |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.3] screen contract — 10 filter
  fields, 44-column grid, 9 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.1] scenario — caller-scoped master
  orphan listing
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-01 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen reference for `ResolveCharityScope`
- [Source: Backend/src/IIROSA.Application/DTOs/OfficeProjectManagement/OfficeProjects.cs:187]
  paged-result shape to copy
- [Source: Backend/src/IIROSA.Api/Controllers/OrphanPaymentsController.cs:68] batch-numbers
  lookup; [LookupManagementController.cs:267/388] regions / centers-by-region lookups
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] latest greenfield
  skeleton story — DI spot, serial formula, verification caveats
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] envelope / data-list
  platform rulings

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, 2026-08-24).

### Debug Log References

- `dotnet build Backend/IIROSA.sln` — Build succeeded, 0 errors (68 warnings, all pre-existing).
- Live smoke on a private instance (`127.0.0.1:60971`, `--no-build`, user's live API untouched):
  anonymous POST → **401**; SuperAdmin POST → **200** camelCase envelope
  (`items/totalCount/page/pageSize/totalPages`), age computed server-side (2016-06-06 birth →
  `age: 10`), guardian resolved via Father fallback, `detailedAddress`/`mobileNumber` from Family;
  `ageTo < ageFrom` → **400** `{"message":"One or more fields are invalid","errors":{"AgeTo":"AgeTo must be greater than or equal to AgeFrom"}}`;
  age filter 8–12 → correct subset. `?????` in one FullName is the Git-Bash codepage artifact,
  not stored data (known caveat).
- `npm run build` — browser bundle generation complete (TypeScript + templates clean); the first
  run stalled >20 min with zero node processes because a parallel session's ng build raced on
  `.angular/cache` — killed and rerun.

### Completion Notes List

- **Reality check was stale on both ends** — a parallel session (UC-FAM-14) had already created
  `ReportsController` + `ReportSheetService` and the frontend `modules/reports/` host (epic 5).
  Implemented **additively**: my `orphans` endpoint joined the existing controller (which
  inherits `ApiController`, not the story's `ControllerBase` — functionally equivalent: same
  `[ApiController] [Authorize] [Route]` surface); the frontend module/routing/model/service were
  extended, not created. During implementation another session began adding UC-ORR-14
  (`non-renewed-reports`) **on top of this skeleton** — 18-1 code verified intact after their
  edits (the reuse mandate working as designed).
- **DI needs no code** — story said "register beside the mission lines (~163)", but
  `RegisterApplicationServices` (Application/ServiceCollectionExtensions.cs:240) dynamically maps
  `*Service` → `I*Service` and `AddValidatorsFromAssembly` registers validators; both live via
  `Program.cs:93`. `ReportService`/`IReportService`/`OrphanDataFilterValidator` resolve by
  convention (verified by the 200 response).
- **Dev Note "global query filter" is wrong** (epic-16 review finding, re-proven here): the
  platform has NO global soft-delete filter — the service filters `!o.IsDeleted` explicitly
  (plus `!item.IsDeleted`/`!OrphanPayment.IsDeleted` in the batch subquery).
- **Model file** is the existing `models/report.model.ts` (epic 5), not the story's
  `report.models.ts` — extending the real file beats a duplicate.
- **Column count**: the §23.S.3 labels enumerate 42 DTO keys (43 with serial, rendered in the
  grid as `(page-1)*size+i+1`) vs the spec's "44" — the discrepancy is label-count drift in the
  legacy screen, recorded here; no column invented to force the count.
- **Degenerate exclusion sets** (no backing columns, epic no-migration ruling): `Excluded=true`
  returns an empty result with an Information log; `NotExcluded` (default) excludes nobody.
  `IsFinishedSponsorship` accepted on the wire, NOT applied — semantics land with 18-4.
- **Charity pinning**: `ApplyCharityScopeAsync` pins by `ICurrentUserService.CharityId` claim
  first, HQ explicit narrow second, `CountryId` charity-set third. The seeded
  `Charity@IIROSA.com` user carries NO CharityId/CountryId claim (token decoded to verify), so
  it falls through unconstrained — the OfficeProject reference behavior, not a pin failure; any
  real charity user (with the claim) is pinned.
- **Frontend notes**: the shared `report-viewer/` shell hosts filter-panel + grid via
  `<ng-content select="[report-filters|report-grid]">` slots with بحث / استخراج البيانات /
  shared Pagination; export pages the whole selection server-side (200 cap) into one workbook;
  sidebar التققارير section gate moved from `Families.FollowUp` to `Reports.View` with per-item
  gates (same role set — no access change for existing users). OnPush omitted per story ruling.
- **Tests excluded** per the standing user decision.

### File List

Backend (new): `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` · `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` · `Backend/src/IIROSA.Application/Services/ReportService.cs` · `Backend/src/IIROSA.Application/Profiles/ReportProfile.cs` · `Backend/src/IIROSA.Application/Validators/Reports/OrphanDataFilterValidator.cs`
Backend (edited): `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` (additive `orphans` action + `IReportService` ctor param)
Frontend (new): `Frontend/src/app/modules/reports/report-viewer/*` (4 files) · `Frontend/src/app/modules/reports/orphan-data-report/*` (4 files) · `Frontend/src/app/modules/reports/services/report-export.service.ts`
Frontend (edited): `Frontend/src/app/modules/reports/models/report.model.ts` · `Frontend/src/app/modules/reports/services/report.service.ts` · `Frontend/src/app/modules/reports/reports-routing.module.ts` · `Frontend/src/app/modules/reports/reports.module.ts` (task-claimed, previously unlisted) · `Frontend/src/app/core/services/auth.service.ts` · `Frontend/src/app/layouts/main-layout/main-layout.component.html` · `Frontend/src/assets/i18n/ar.json` + `en.json`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-01 and module spec §23.S.3 / §23.U.1; greenfield status, epic skeleton scope and no-migration ruling verified against the codebase. |
| 2026-08-24 | Implemented (skeleton extended additively around the parallel session's epic-5/UC-FAM-14 Reports code); backend solution 0 errors, live smoke passed (401/200/400 + scoping), frontend bundle generation clean; status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
