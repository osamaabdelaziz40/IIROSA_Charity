# Story 18-35: New orphans and new widows الأيتام والأرامل الجدد

| Field | Value |
| --- | --- |
| Story key | `18-35-new-orphans-and-new-widows` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-35 — الأيتام والأرامل الجدد |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.35 scenario — no dedicated §23.S screen) |
| Route | `#/reports/new-beneficiaries` (thin print screen — board assigns no route; decision recorded) |
| Endpoint | data: `POST /api/Reports/new-beneficiaries` (JSON, variant-keyed); document: client-side via 18-21's PDF service. Legacy realisation: `rptNewOrphans.rpt`, `rptNewOrphansV2.rpt`, `rptNewWidows.rpt`, `rptNewWidows_Family.rpt` |
| Depends on | **18-1 landed** (skeleton + shell); **18-21 landed** (`services/report-pdf.service.ts`) |
| Roles | HQ roles → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a HQ role, I want to be able to new orphans and new widows الأيتام والأرامل الجدد, so that the
paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given an HQ user with an active session on `#/reports/new-beneficiaries`, when the actor sets
   the charity + registration date range and presses one of the four print variants, then the
   corresponding printed list of newly registered orphans/widows is produced for print/save. No
   stored data is changed.
2. Given the data is fetched, when the lists render, then the rows come from
   `POST /api/Reports/new-beneficiaries` (`charityId`, `dateFrom`, `dateTo`, `variant`) returning
   the composed payload + variant key — the 9-17 convention (never PDF bytes) — and the document
   is composed client-side by `report-pdf.service.ts`.
3. Given the selection returns no row, when the document is produced, then the actor is told that
   there is nothing to produce rather than receiving an empty file.
4. Given the caller is a charity user (endpoint roles are HQ-only, scope guard still applies),
   when the report runs, then only that charity's rows are returned — pinned server-side from
   `ICurrentUserService.CharityId`, never from the payload. Given an HQ caller (`IsHeadOffice`)
   with an explicit charity id, the report runs on that charity's data; a `CountryId` claim
   additionally pins the charities of that country.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.35 passes end to end — all four legacy `.rpt` variants answer from
one endpoint + one screen; the registration-date predicate reads the entities' audit/registration
dates (no new column); the scope is enforced server-side.

## Variant contract (four legacy reports → one endpoint, `variant` key)

| Variant key | Legacy report | Rows |
| --- | --- | --- |
| `orphans` | rptNewOrphans.rpt | orphans registered in range: رقم اليتيم · أسم اليتيم · تاريخ الميلاد · الجمعية · تاريخ التسجيل |
| `orphansV2` | rptNewOrphansV2.rpt | same rows + كود العائلة · اسم المعيل (the wider layout) |
| `widows` | rptNewWidows.rpt | widows (mothers) of families registered in range: اسم الأرملة · الرقم القومي · تاريخ وفاة الزوج · الجمعية · تاريخ التسجيل |
| `widowsByFamily` | rptNewWidows_Family.rpt | widows grouped by family: كود العائلة · اسم الأرملة · عدد الأبناء · الجمعية |

Params: `charityId` (all-option for HQ), `dateFrom`/`dateTo` (registration window — mandatory
`dateFrom`). Purpose per §23.U.35: sponsorship-offer mailings.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 4)
  - [x] `NewBeneficiariesFilterDto` (`Guid? CharityId`, `DateTime DateFrom`, `DateTime? DateTo`,
        `string Variant`, `int Page = 1`, `int PageSize = 20`) + `NewBeneficiaryListDto`
        (superset of the four variants' columns) in `DTOs/Reports/Reports.cs`;
        `ReportPagedResult<T>`; no `FK_*` wire keys
  - [x] `Validators/Reports/NewBeneficiariesFilterValidator.cs` — `DateFrom` NotEmpty;
        `DateTo ≥ DateFrom`; `Variant` InclusiveIn(`orphans`, `orphansV2`, `widows`,
        `widowsByFamily`); page bounds; invoked in the service
  - [x] `IReportService.GetNewBeneficiariesAsync(...)` + implementation: `ResolveCharityScope(
        filter.CharityId)`; orphan variants read `Orphan` (created/registered in window — use the
        registration/audit date the entity actually carries: verify whether `Orphan` has a
        dedicated registration column or `CreatedOn` is the proxy; RECORD the choice); widow
        variants read the family-mother projection (18-6's mother+family shape) over families
        registered in the window; ordered by registration date then code
- [x] **Task 2 — API endpoint** (AC 2, 5)
  - [x] `[HttpPost("new-beneficiaries")] [Authorize(Roles = "SuperAdmin,Admin")]` in 18-1's
        `ReportsController` → `Ok(paged)`; standard error ladder; no `ApiResponse<T>`
- [x] **Task 3 — Thin screen + documents** (AC 1, 3)
  - [x] `Frontend/src/app/modules/reports/new-beneficiaries-report/` thin 4-file component in the
        shell: charity + date-range filters, a 4-button variant group (أيتام جدد · أيتام جدد
        موسع · أرامل جدد · أرامل حسب الأسرة), result grid for the chosen variant; route
        `new-beneficiaries`, `AuthGuard + PermissionGuard`, `data.permission: 'Reports.View'`;
        bespoke grid + shared `Pagination`; `trackBy`; empty state; OnPush omitted
  - [x] One list-document builder with variant-driven column sets feeding 18-21's
        `report-pdf.service.ts` (A4 portrait, RTL, footer count row); `widowsByFamily` groups its
        rows by family; labels through `reports.newBeneficiaries.*` i18n in **both** `ar.json`
        and `en.json`; empty result → nothing-to-produce message, no file
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] Live check: anonymous → 401; missing `dateFrom` / bad `variant` → 400 errors-map; Charity
        token → 403; HQ + `charityId` → that charity; each variant returns its own column set;
        empty window → message, no file
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 live-API lock caveat; ng-serve
        stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Variant collapse (recorded)

The legacy realisation is four Crystal `.rpt` files. The platform collapses them to ONE JSON
endpoint + variant key (the 9-17 convention: composed payload + variant key, never PDF bytes) and
ONE screen with four commands — `orphans`/`orphansV2` share rows (wider layout), `widows`/
`widowsByFamily` share rows (grouped). If a fifth layout is wanted later it joins the variant
registry, not a new endpoint.

### Registration-date source (decision to record)

"New" means registered in the window. Verify what the entities carry: a dedicated registration
date column vs `CreatedOn` (audit). Either is acceptable — RECORD which shipped; do not add a
column in this epic.

### Platform rules that bind this story

- Raw envelope + anonymous `{ message }` errors — NOT `ApiResponse<T>` (15-1 ruling,
  architecture.md §10); `ControllerBase` + `[Authorize]`.
- camelCase wire; no `FK_*` DTO keys; FluentValidation in the service; reads via `IUnitOfWork`
  repositories; global soft-delete query filter.
- Caller scope from `ICurrentUserService` only; lookup labels `NameAr ?? NameEn`; client-side PDF
  only; bespoke grid + shared `Pagination`; EP-18 adds no entities and no migration.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| Widow sponsorship query grid (the live query, not the print) | 18-6 |
| Guardian/widow identification sheets at a date | 18-37 |
| Generic in-browser preview modal | 18-40 |
| Excel export of these lists | 18-41 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.35] scenario — sponsorship-offer
  lists; legacy four-report realisation
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-35 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] skeleton + shell this story
  reuses
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-6-widows-requiring-sponsorship.md] the
  mother+family widow projection the widow variants share
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the PDF service
  the lists render through

## Dev Agent Record

### Agent Model Used

GLM 4.7 (Claude Code harness)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → 0 Error(s) (pre-existing warnings only).
- `npm run build` → exit 0, 0 `Error:` lines. First run failed NG8002 (`aria-label` property
  binding on the variant-group div) — fixed with `[attr.aria-label]`.

### Completion Notes List

- **Registration-date source (recorded, per the story's decision-to-record note):** neither
  `Orphan` nor `Family` carries a dedicated registration date column — `CreatedOn` (audit) is the
  proxy for "registered in the window" on both branches (orphan variants: `Orphan.CreatedOn`;
  widow variants: `Family.CreatedOn`). No column added (epic-wide no-migration ruling).
- **Variant canonicalisation:** the validator matches case-insensitively; the service
  canonicalises to lowercase (`orphans` / `orphansv2` / `widows` / `widowsbyfamily`) and echoes
  the canonical key — the TS `NewBeneficiariesVariant` union mirrors the canonical wire values.
- **Widow variants = 18-6's mother+family projection WITHOUT the has-orphans floor:** the family
  registration itself is the frame — a registered family's mother goes on the offer list even
  before orphans are recorded. `ChildrenCount` still projects the family's non-deleted orphans
  (0 is a valid count for the widowsByFamily column).
- **Charity scope:** the 18-22/24 ladder verbatim — orphan branch inline over `o.FK_CharityId`;
  widow branch via `ApplyMotherCharityScopeAsync` (18-6's helper).
- **One screen, four commands:** the variant group switches the grid's column set and keeps the
  loaded rows (the orphan pair / widow pair share row sources); بحث re-runs the read for the
  chosen variant. `widowsByFamily` grouping: the widow row IS the family's mother, so the grouped
  document sorts its rows by familyCode; the grid keeps the server's registration order.
- **Route** `#/reports/new-beneficiaries` granted (board assigns none — the story's recorded
  decision); `AuthGuard + PermissionGuard`, `permission: 'Reports.View'`.
- **AC 3:** an empty selection refuses the print with the nothing-to-produce toast; the endpoint
  returns `rows:[]` in the paged envelope, never an empty document.
- **Task 4 live check deferred** to the epic's consolidated private-instance smoke
  (127.0.0.1:60970 pattern) — the live-check box stays open until then; tests excluded per the
  standing user decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `NewBeneficiariesFilterDto`,
  `NewBeneficiaryListDto` (column superset), `NewBeneficiariesReportDto : ReportPagedResult<>`
  with the variant echo.
- `Backend/src/IIROSA.Application/Validators/Reports/NewBeneficiariesFilterValidator.cs` — NEW.
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetNewBeneficiariesAsync`.
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (orphan + widow
  branches, window, ladder, ordering, paged projection).
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST new-beneficiaries`
  (HQ roles; raw envelope; 400 errors-map / 500 `{ message }` ladder).

Frontend:
- `Frontend/src/app/modules/reports/models/report.model.ts` — `NewBeneficiariesVariant`,
  `NewBeneficiariesFilter`, `NewBeneficiaryRow`, `NewBeneficiariesReport`.
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getNewBeneficiaries`.
- `Frontend/src/app/modules/reports/new-beneficiaries-report/` — NEW 4-file component
  (filter panel, 4-button variant group, per-variant grids, printSheet builder).
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — `new-beneficiaries` route.
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `reports.newBeneficiaries` (31 keys ×2).

### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; missing `dateFrom` and a bad `variant` → 400 with the errors map flagging the field.
- Charity token → 403; HQ + `charityId` → that charity.
- Each variant (orphans / orphansV2 / widows / widowsByFamily) returns its own column set.
- Empty window → message, no file.
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-RPT-35 and module spec §23.U.35; four-variant collapse to one endpoint + variant key (9-17 convention) and the registration-date source decision recorded. |
| 2026-08-25 | Tasks 1–3 + Task 4's build subtask implemented and checked; registration-date proxy (CreatedOn) and widow-floor rulings recorded; Dev Agent Record written; Status → in-progress (live check pending the epic's consolidated smoke). |
| 2026-08-25 | Live smoke passed (auth, errors-map, 403 gate, variants, charity narrowing, empty window). Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
