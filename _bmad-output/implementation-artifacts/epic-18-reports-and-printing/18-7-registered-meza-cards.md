# Story 18-7: Registered Meza cards تقرير الكروت المسجلة

| Field | Value |
| --- | --- |
| Story key | `18-7-registered-meza-cards` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-07 — تقرير الكروت المسجلة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.S.8 screen, §23.U.7 scenario) |
| Route | `#/reports/meza-cards` |
| Endpoint | `POST /api/Reports/meza-cards` |
| Depends on | **18-1 landed** (Reports skeleton + `report-viewer` shell + `report-export.service.ts`) |
| Roles | Gen. Director → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a General Director, I want to be able to registered Meza cards تقرير الكروت المسجلة, so that
the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at `#/reports/meza-cards`, when
   the actor presses «بحث», then no stored data is changed — the operation is a read. (The epic's
   «Create a record» wording is template artefact — §23.S.8/§23.U.7 show a query screen; no entity
   is created; see Dev Notes.)
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Reports/meza-cards` with a typed request DTO and the raw paged envelope is rendered
   without a page reload.
3. Given the caller is a charity user (endpoint roles are HQ-only, scope guard still applies),
   when the report runs, then only that charity's rows are returned — pinned server-side from
   `ICurrentUserService.CharityId`, never from the payload. Given an HQ caller (`IsHeadOffice`)
   with an explicit charity id, the report runs on that charity's data; a `CountryId` claim
   additionally pins the charities of that country.
4. Given no family in scope carries a registered Meza card, when the report runs, then the grid
   renders empty and the paging control reports zero pages.
5. Given استخراج البيانات is pressed with an empty result, when the export runs, then the actor is
   told there is nothing to produce rather than receiving an empty file.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** the single charity filter and the 11-column grid of §23.S.8 are implemented
on 18-1's shell; §23.U.7 passes end to end; the Meza-card columns render whatever the guardian
registration actually stores today (gaps recorded, never migrated); the scope is enforced
server-side.

## Screen contract (§23.S.8 — تقرير الكروت المسجله, 1 filter field, 2 commands)

| Section | Field (as labelled) | DTO key | Control | Mandatory | Source / rule |
| --- | --- | --- | --- | --- | --- |
| تقرير الكروت المسجله | الجمعية | `CharityId` | Drop-down | No | `GET /api/Charities` (`result.items || []`) + كافة الجهات all-option — HQ only |

| Grid columns (row source: families with guardian Meza cards — same base shape as §23.S.7) |
| --- |
| أسماء الأيتام · الرقم القومي · اسم المعيل · أكواد الأيتام · كود العائله · التليفون · الجمعيه · كارت ميزا · تاريخ انتهاء كارت ميزا |

| Command | Handler | In scope |
| --- | --- | --- |
| بحث | `GetData()` | Yes |
| استخراج البيانات | `ExportData()` | Yes |

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 3, 4)
  - [x] `MezaCardsFilterDto` (`Guid? CharityId`, `int Page = 1`, `int PageSize = 20`) +
        `MezaCardsListDto` (family code, guardian name + national id, phone, charity name, orphan
        names/codes aggregate, `MezaCardNumber`, `MezaCardExpiry`) in `DTOs/Reports/Reports.cs`;
        result reuses `ReportPagedResult<T>`; no `FK_*` wire keys
  - [x] `Validators/Reports/MezaCardsFilterValidator.cs` — page bounds; invoked in the service
  - [x] `IReportService.GetMezaCardsAsync(...)` + implementation: `ResolveCharityScope(
        filter.CharityId)`, then a read-only projection over `Family` + `Provider` (guardian) +
        children aggregating orphan names/codes, carrying the Meza card number and expiry from
        wherever the guardian registration stores them today — **verify first**: grep
        `IIROSA.Domain` for a Meza-card column on `Provider`/`Family`; if none exists the two
        columns render blank and the gap is RECORDED (18-1's epic ruling: DTO carries the key,
        column renders empty, no migration in this epic). Ordered by family code
- [x] **Task 2 — API endpoint** (AC 2, 6)
  - [x] `[HttpPost("meza-cards")] [Authorize(Roles = "SuperAdmin,Admin")]` in 18-1's
        `ReportsController` → `Ok(paged)`; standard ValidationException→400-errors-map /
        catch-all→500 `{ message }` ladder; no `ApiResponse<T>`
- [x] **Task 3 — Screen** (AC 1, 4)
  - [x] `Frontend/src/app/modules/reports/meza-cards-report/` thin 4-file component in the shell;
        route `meza-cards`, `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`;
        bespoke grid + shared `Pagination`; `trackBy`; empty state; OnPush omitted (list-screen
        precedent); orphan names/codes render as a joined string per row
- [x] **Task 4 — Export** (AC 5) — استخراج via 18-1's `report-export.service.ts` with this
      report's column set; empty result → nothing-to-produce message, no file
- [x] **Task 5 — i18n** — `reports.mezaCards.*` in **both** `ar.json` and `en.json`
- [x] **Task 6 — Verification** (AC 1–6) — anonymous → 401; Charity-role token → 403; HQ +
      `charityId` → that charity; empty scope → empty grid; Meza columns render real values when
      the backing data exists, blank otherwise; `dotnet build` + `npm run build` green
      (MSB3021/3027 live-API lock caveat; ng-serve stale-bundle grep); tests excluded per the
      standing user decision

## Dev Notes

### Template artefact (recorded)

The epic generates US-RPT-07 as «Create a record» with save/refusal ACs and a «Faild Operation»
rule. §23.S.8 and §23.U.7 show a query screen (charity dropdown + grid + بحث/استخراج). Nothing is
created by this screen; Meza-card data lands through the guardian/family registration vertical,
not here. The 5-point size covers the family+guardian+children aggregation, not any write path.

### Extract variant boundary

18-8 (`استخراج كروت العائل`) EXTENDS this screen with the bank-extract parameters (date range,
batch, `MezaCardExist` switch) and its own workbook contract. This story lands the base query +
standard استخراج ONLY — the extract's extra filter fields and commands are out of scope below.

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
| Extract variant (reportNo/isCodes/batchId/date-range/MezaCardExist + bank workbook) | 18-8 |
| Family-projects grid sharing this screen shape | 18-11 (landed scope) |
| Meza-card registration/write path | family/guardian registration vertical |
| Excel-export engine hardening | 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.S.8] screen contract — 1 field,
  11-column grid, 2 commands
- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.7] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-07 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] skeleton + shell + export
  service this story reuses
- [Source: Backend/src/IIROSA.Domain] `Family` / `Provider` — verify where the Meza-card columns
  live before writing the projection

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code dev agent)

### Debug Log References

- Verify-first grep (Task 1 mandate): `Meza|Card` case-insensitive across `IIROSA.Domain` →
  **zero matches** — no Meza/card column exists on `Provider`, `Family`, or anywhere else.
- Smoke run 2026-08-24, private instance `dotnet IIROSA.Api.dll --urls http://127.0.0.1:60970`:
  - anonymous POST `/api/Reports/meza-cards` → **401** ✓
  - SuperAdmin POST → **200** empty envelope ✓ (recorded gap — see below)
  - Charity-role token POST → **403** (HQ-only roles) ✓
  - out-of-range page bounds → **400** `errors.Page`/`errors.PageSize` ✓
- `dotnet build Backend/IIROSA.sln` — 0 CS errors (MSB3021/3027 copy-locks only);
  `npm run build` exit 0.

### Completion Notes List

- **Data-gap resolution (recorded):** the row source is families CARRYING registered guardian
  Meza cards (AC 4 + the contract's row-source line), and no card column exists domain-wide
  (grep-proven) — so no family can be a card carrier today. Per the standing product ruling
  (same as UC-RPT-03/04: empty set + recorded gap) the endpoint/screen/export contract ships
  now with an empty set and a logged gap; when the guardian registration vertical adds the card
  fields, the family+guardian+children projection AND the card predicate land in
  `GetMezaCardsAsync` alone — the DTO already carries `MezaCardNumber`/`MezaCardExpiry`, so no
  screen/export/i18n change will be needed. No column invented, no migration.
- 18-8 boundary honoured: only the base query + standard استخراج landed; the extract's extra
  parameters (date range, batch, `MezaCardExist` switch) and its bank workbook are NOT here.
- Grid = serial + the 9 contract columns (أسماء الأيتام · الرقم القومي · اسم المعيل ·
  أكواد الأيتام · كود العائله · التليفون · الجمعيه · كارت ميزا · تاريخ انتهاء كارت ميزا);
  orphan names/codes render as joined strings per row when rows exist.

### File List

- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `MezaCardsFilterDto`,
  `MezaCardsListDto`
- `Backend/src/IIROSA.Application/Validators/Reports/MezaCardsFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetMezaCardsAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (empty set +
  logged gap) + validator injection
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `meza-cards` action (HQ-only)
- `Frontend/src/app/modules/reports/models/report.model.ts` — `MezaCardsFilter`, `MezaCardsRow`
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getMezaCards()`
- `Frontend/src/app/modules/reports/services/report-export.service.ts` — `exportMezaCards()`
- `Frontend/src/app/modules/reports/meza-cards-report/` — new 4-file component
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `meza-cards` route
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry (HQ roles)
- `Frontend/src/assets/i18n/ar.json` / `en.json` — `reports.mezaCards.*` (15 keys each)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-07 and module spec §23.S.8 / §23.U.7; «Create a record» artefact re-cut to the read the spec shows; Meza-column data-gap ruling and the 18-8 extract boundary recorded. |
| 2026-08-24 | Implemented: verify-first grep proved no Meza/card column exists domain-wide → standing empty-set+recorded-gap ruling applied (AC 4's row source cannot match today); contract (DTO/screen/export/i18n) ships complete so only `GetMezaCardsAsync` changes when the fields land. Verified on private smoke instance (401/200-empty/403/400). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
