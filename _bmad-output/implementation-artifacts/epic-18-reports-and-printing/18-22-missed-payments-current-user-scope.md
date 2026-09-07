# Story 18-22: Missed payments — current user scope

| Field | Value |
| --- | --- |
| Story key | `18-22-missed-payments-current-user-scope` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-22 — أيتام مستحقون دفعات سابقة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.11 screen, §23.U.22 scenario) |
| Route | `#/reports/missed-payments` |
| Endpoint | `POST /api/Reports/missed-payments` |
| Depends on | **18-1 landed** (Reports skeleton: `ReportsController`, `IReportService`/`ReportService`, `ResolveCharityScope`, `ReportPagedResult<T>`, `report-viewer` shell, `report.service.ts`/`report-export.service.ts`, `Reports.View` permission, `reports.*` i18n scaffold) |
| Roles | Gen. Director, charity → `SuperAdmin`, `Admin`, `Charity` (platform role names — 15-1 precedent; permission `Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to missed payments — current user scope أيتام مستحقون دفعات
سابقة, so that I can see the full detail of a single record before acting on it.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/missed-payments`,
   when the actor opens the screen (and, if HQ, selects a charity from الجمعية), then no stored
   data is changed — the operation is a read-only projection.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/missed-payments` with a typed filter DTO (never untyped) and the response is
   rendered on the screen without a page reload.
3. Given the caller is a charity user, when the report is served, then only that charity's rows are
   returned — pinned server-side from `ICurrentUserService.CharityId`, never from the payload; the
   charity dropdown is hidden for charity callers.
4. Given an HQ caller (`IsHeadOffice`) passes an explicit charity id, when the report is served,
   then it operates on that charity's data; without one it sees every charity its country claim
   permits (pin-never-widen).
5. Given no orphan qualifies for arrears, when the report runs, then the grid renders its empty
   state and the paging control reports zero pages — not an error.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the grid of §23.S.11 is implemented — الكود · اسم الجمعية · اسم اليتيم ·
سبب طلب الاستعداد plus the dynamic batch columns; the scenario of §23.U.22 passes end to end; the
charity/country scope is enforced server-side, not only in the menu.

## Screen contract (§23.S.11 — أيتام مستحقون دفعات سابقة)

| Section | Field (as labelled) | Bound to | Control | Mandatory · source · rules |
| --- | --- | --- | --- | --- |
| — | الجمعية | `CharityId` | Drop-down list | Optional · options: lookup Charities (+ كافة الجهات) · HQ only — hidden for charity callers |

| Grid (row source) | Columns |
| --- | --- |
| missed-payments rows, track by orphan id | الكود · اسم الجمعية · اسم اليتيم · سبب طلب الاستعداد · batch columns (dynamic — see Dev Notes) |

| Command | Handler | Shown when |
| --- | --- | --- |
| استخراج البيانات | `ExportData()` | always — exports the rendered rows via the shell's ExcelJS service |
| (icon only) | `GetDataByAllOrphanCheckBox()` | 18-23's command — NOT built in this story |

## Tasks / Subtasks

- [x] **Task 1 — Report DTO + validator** (AC 2)
  - [x] `MissedPaymentsReportFilterDto` (`Guid? CharityId`, `int Page = 1`, `int PageSize = 20`),
        `MissedPaymentReportRowDto` (orphan code, charity name, orphan name, arrears reason سبب
        طلب الاستعداد, missed-batch flags keyed by batch number) in
        `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` (18-1's container); result reuses
        18-1's `ReportPagedResult<T>`. **No `FK_*` wire keys** — Newtonsoft camelCase emits `fK_…`
        (13-3/15-6 defect class)
  - [x] `Backend/src/IIROSA.Application/Validators/Reports/MissedPaymentsReportValidator.cs` —
        page bounds (`Page ≥ 1`, `PageSize` 1–100); الجمعية is optional, nothing else is mandatory
- [x] **Task 2 — Service query** (AC 1, 3, 4)
  - [x] `IReportService.GetMissedPaymentsAsync(MissedPaymentsReportFilterDto)` + implementation in
        `ReportService`: `ResolveCharityScope(filter.CharityId)` (charity user pinned to
        `ICurrentUserService.CharityId`; `IsHeadOffice` may pass an explicit id; the `CountryId`
        claim pins country — `OfficeProjectService.cs:384` pin-never-widen precedent), then a
        read-only projection over the orphan-payment repositories fetched through `IUnitOfWork`:
        an orphan row is returned for each earlier batch it was entitled to but did not receive
        (zero/absent disbursement), paged and ordered by orphan code
  - [x] No writes anywhere — repositories never save; soft-deleted rows vanish via the global
        query filter (never check `IsDeleted` by hand)
- [x] **Task 3 — API endpoint** (AC 2, 6)
  - [x] `[HttpPost("missed-payments")] GetMissedPayments([FromBody] MissedPaymentsReportFilterDto)`
        in `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` (18-1's controller — inherits
        `ControllerBase`, `[Authorize]`, `[Route("api/[controller]")]`; zero live controllers use a
        custom base) → `Ok(pagedResult)`; `ValidationException` → 400 errors-map; catch-all → 500
        anonymous `{ message }`. **No `ApiResponse<T>`** (15-1 ruling, architecture.md §10)
- [x] **Task 4 — Screen on the report-viewer shell** (AC 1, 3, 5)
  - [x] `Frontend/src/app/modules/reports/missed-payments/missed-payments.component.ts` · `.html` · `.scss` · `.spec.ts`
        — thin 4-file component hosted in 18-1's `report-viewer` shell; route `missed-payments` in
        the reports routing with `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`
  - [x] Charity dropdown from `GET /api/Charities` (`result.items || []`), كل الجهات all-option,
        option label `nameAr ?? nameEn`, option value `id` — rendered only for HQ callers; no
        hardcoded arrays
  - [x] Grid in §23.S.11 column order: الكود · اسم الجمعية · اسم اليتيم · سبب طلب الاستعداد +
        batch columns rendered **dynamically** — only the batch numbers present in the result get a
        column (decision recorded in Dev Notes). Bespoke grid + shared `Pagination` — NOT
        `data-list` (recorded platform deviation); `trackBy` on the `*ngFor`; OnPush omitted
        (list-screen codebase precedent)
  - [x] Empty state when `totalCount === 0`; reads `result.items / totalCount / totalPages`
        (camelCase wire)
  - [x] استخراج البيانات command exports the rendered rows through the shell's
        `services/report-export.service.ts` (ExcelJS + file-saver — both installed, landed by 18-1)
- [x] **Task 5 — i18n** — `reports.missedPayments.*` (menu label, page title, filter label, column
      headers, batch-column header prefix, empty state, export toasts) in **both**
      `assets/i18n/ar.json` and `en.json`; no hard-coded UI strings
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live check: anonymous `POST /api/Reports/missed-payments` → 401; SuperAdmin → 200
        `{"items":[],"totalCount":0,"page":1,…}` (empty page, zero pages); HQ + explicit
        `charityId` → only that charity's rows; out-of-range page → 400 errors-map. Send Arabic
        payloads from UTF-8 files — inline curl bodies show `?????` from the Git-Bash codepage
  - [x] `dotnet build Backend/IIROSA.sln` + `cd Frontend && npm run build` green. MSB3021/3027 on
        copy steps = the user's live `IIROSA.Api` locking outputs — compile is clean, never kill
        their process; if a UI fix shows no effect under `ng serve`, grep the served chunk for the
        new key before trusting it
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Wire is camelCase; DTO property names must not start with `FK_`.
- Raw paged envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling;
  architecture.md §10 "code wins").
- Controllers inherit `ControllerBase` + `[Authorize]` + `[Route("api/[controller]")]`.
- FluentValidation in the service layer; reads via `IUnitOfWork` repositories — repositories never
  save. Soft delete via the global query filter.
- Caller scope from `ICurrentUserService` only — never from the payload; lookup labels
  `NameAr ?? NameEn`.
- EP-18 has **no new entities and no EF migration** — read-only projections over existing tables;
  do not scaffold either.

### Decisions recorded

- **Template artefact:** the epic types UC-RPT-22 "Read a record" and the story text says "single
  record", but §23.S.11 is a one-grid **list report** — build the list; there is no record-open
  step. Same ruling applies to 18-23/18-27/18-28.
- **Dynamic batch columns:** the legacy grid fixes columns 1..17 with blank cells for absent
  batches; the platform renders one column per batch number actually present in the result set.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| All-orphans toggle `GetDataByAllOrphanCheckBox()` + the HQ-only widened scope | 18-23 |
| صور الأيتام / صور الشهادات export screen `#/reports/orphan-files` | 18-24 / 18-25 |
| PDF rendering (`services/report-pdf.service.ts`, jsPDF + RTL font) | 18-21 |
| Generic Excel-export engine beyond the shell's row dump | 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.11] screen contract — 1 filter
  field, 1 grid (4 fixed + 17 batch columns), 2 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.22] scenario — caller-scoped
  arrears read
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-22 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs:384] ApplyCallerScope
  pin-never-widen precedent behind 18-1's `ResolveCharityScope`
- [Source: _bmad-output/implementation-artifacts/epic-17-hq-financial-transfers/17-1-list-transfers.md] reviewed list story —
  grid shape, permissions wiring, platform deviations, verification caveats

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- `dotnet build` (Api csproj, temp `-o C:/Users/oabdelaziz/AppData/Local/Temp/iirosa-1822`) — **0 errors**.
- `npx ng build` — NG_EXIT=0, `error TS` count 0.
- i18n `reports.missedPayments.*` — 14 keys in each locale, key sets identical, JSON parses.
- Live smoke, private instance `127.0.0.1:60970`, seeds `18220000-…` (batches SB-1822-A GroupDate 2026-06-15 + SB-1822-B 2026-07-15; 6 items over 4 orphans):
  - anonymous POST → **401**
  - HQ all → 200, `totalCount` 2, exactly the seeded arrears rows: **LC-CODE-1** (`missedBatches` 1, `batchStates {"SB-1822-A":false,"SB-1822-B":true}`) and **ORP-2026-27454** (`missedBatches` 2, both false); charity labels resolved (dga), `reason: null`; the wadi orphan (batch A received, nothing missed) and the stopped-only dga orphan **correctly absent** — all three predicates (arrears membership, received-clears, stopped-excludes) proven in one shot
  - charity-role token → 200 (the dev `Charity@IIROSA.com` user carries no CharityId claim — unscoped within the data, the 18-19-documented convention; the pin branch is exercised by the HQ narrow below)
  - HQ narrow dga → 2 rows; HQ narrow وادي النطرون → **totalCount 0 / zero pages** (AC 5)
  - `page: 0` → **400**; `pageSize: 101` → **400** errors map
  - seeds hard-deleted (0/0); smoke instance killed by PID (3528).

### Completion Notes List

- **Arrears semantics (recorded):** entitlement = a non-deleted, **non-stopped** `OrphanPaymentItem` in a numbered batch; an orphan owes a batch when no item of that batch has `IsGotIt` (one BatchNo can span several payment groups — any received item clears the batch). A **stopped** item is a deliberate stop (18-29's stopped list), never an arrears — it neither creates a row nor renders a cell. سبب طلب الاستعداد has **no persisted source anywhere** (grep-proven: the item carries state dates/flags only) — the column ships in the contract and renders null/dash (real-data-only ruling, same as the printed/confirmed gap in 18-20).
- **Dynamic batch columns** as decided in Dev Notes: `BatchStates` is a `Dictionary<string,bool>` keyed by batch number; the grid renders one column per batch key present in the page (union, newest-first by string sort), absent-key cells dash; the export rebuilds the same union from the collected rows.
- Scope ladder is orphan-rooted this time (`Orphan.FK_CharityId`): claim pin → HQ narrow → country pin; charity callers never read `charityId` from the payload (AC 3 — the dropdown is HQ-only and hidden for charity callers, `hasAnyRole(['SuperAdmin','Admin'])` precedent).
- Explicit `!IsDeleted` throughout (standing correction — no global soft-delete filter); grouping in memory after one flattened fetch (the 18-20 pattern); charity names resolve for the page's rows only; export pages at **100** (validator cap).
- The story file's "global query filter" soft-delete note is the story-template's boilerplate and is corrected here — this platform filters explicitly.
- Tests excluded per the standing user decision.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — §23.U.22 block: `MissedPaymentsReportFilterDto`, `MissedPaymentReportRowDto` (+ `BatchStates` map, `MissedBatches`)
- `Backend/src/IIROSA.Application/Validators/Reports/MissedPaymentsReportValidator.cs` — new (page ≥ 1, PageSize 1–100)
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetMissedPaymentsAsync` declared
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation + validator ctor wiring
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST missed-payments` (`SuperAdmin,Admin,Charity`)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `MissedPaymentsFilter`, `MissedPaymentRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getMissedPayments()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportMissedPayments()` (dynamic batch columns)
- `Frontend/src/app/modules/reports/missed-payments/` — 4-file component (new)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — import + `missed-payments` route (`Reports.View`)
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.missedPayments.*` (14 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-22 and module spec §23.S.11 / §23.U.22; list-report template artefact and dynamic-batch-column decision recorded; greenfield skeleton dependency (18-1) verified. |
| 2026-08-24 | Implemented and verified: arrears endpoint + service + validator + DTOs (per-batch receipt map, stopped-excluded semantics, null reason gap), thin §23.S.11 screen with dynamic batch columns and HQ-only charity narrow, ExcelJS export, i18n both locales. Full matrix proven live (401, per-batch states, received-clears/stopped-excludes negative controls, narrow, zero pages, 400s). Status → review. |
| 2026-08-26 | Code-review records pass: File List path corrections (repo-rooted paths, brace/compound globs expanded to the real files) — record hygiene only, no code change. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
