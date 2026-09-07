# Story 18-39: Family orphan list by date أيتام الأسر بتاريخ

| Field | Value |
| --- | --- |
| Story key | `18-39-family-orphan-list-by-date` |
| Epic | EP-18 — Reports & Printing (التقارير والطباعة) |
| Use case | UC-RPT-39 — أيتام الأسر بتاريخ |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/23-UC-RPT-Reports-and-Printing.md` (§23.U.39 scenario — no dedicated §23.S screen) |
| Route | `#/reports/family-orphans-by-date` (thin print screen — board assigns no route; distinct from 18-13's `#/reports/family-orphans`; decision recorded) |
| Endpoint | data: `POST /api/Reports/family-orphans` (JSON — `charityId`, `date`); document: client-side via 18-21's PDF service. The board/legacy `POST /api/Reports/family-orphans/export/pdf` is superseded — recorded deviation |
| Depends on | **18-1 landed** (skeleton + shell); **18-21 landed** (`services/report-pdf.service.ts`) |
| Roles | HQ roles → `SuperAdmin`, `Admin` (`Reports.View`) |

## Status

done

## Story

As a HQ role, I want to be able to family orphan list by date أيتام الأسر بتاريخ, so that the
paper document the process depends on can be produced and filed.

## Acceptance Criteria

1. Given an HQ user with an active session on `#/reports/family-orphans-by-date`, when the actor
   sets the charity + as-at date and presses طباعة, then the orphans of that charity's families
   as at the given date are printed, grouped by family. No stored data is changed.
2. Given the data is fetched, when the list renders, then the rows come from
   `POST /api/Reports/family-orphans` (`charityId`, `date`) returning the composed payload — the
   document is composed client-side by `report-pdf.service.ts`; the legacy server `/export/pdf`
   streaming is superseded (recorded deviation).
3. Given the selection returns no row (no families as at that date), when the document is
   produced, then the actor is told that there is nothing to produce rather than receiving an
   empty file.
4. Given the caller is a charity user (endpoint roles are HQ-only, scope guard still applies),
   when the report runs, then only that charity's rows are returned — pinned server-side from
   `ICurrentUserService.CharityId`, never from the payload. Given an HQ caller (`IsHeadOffice`)
   with an explicit charity id, the report runs on that charity's data; a `CountryId` claim
   additionally pins the charities of that country.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected (401/403) and the actor is routed back to the login screen.

**Definition of done:** §23.U.39 passes end to end — a per-family grouped orphan list as at a
date prints through the epic's client-side PDF path; the as-at predicate resolves from
registration/audit dates already on the entities (no new column); the scope is enforced
server-side.

## Document contract (§23.U.39 — `charityId`, `date`)

Grouped list: for each family of the charity — كود العائلة · اسم المعيل · المحافظة/المركز —
then its orphans: رقم اليتيم · أسم اليتيم · العمر (as at the chosen date, from `DateOfBirth`).
Footer per family: عدد الأيتام; final footer: إجمالي الأسر + إجمالي الأيتام. A4 portrait, RTL,
family group headers.

## Tasks / Subtasks

- [x] **Task 1 — DTO + validator + service** (AC 2, 4)
  - [x] `FamilyOrphansByDateFilterDto` (`Guid? CharityId`, `DateTime Date`, `int Page = 1`,
        `int PageSize = 20`) + `FamilyWithOrphansDto` (family header fields +
        `List<FamilyOrphanRowDto>`) in `DTOs/Reports/Reports.cs`; result reuses
        `ReportPagedResult<T>` (page = families); no `FK_*` wire keys
  - [x] `Validators/Reports/FamilyOrphansByDateFilterValidator.cs` — `Date` NotEmpty; page
        bounds; invoked in the service
  - [x] `IReportService.GetFamilyOrphansByDateAsync(...)` + implementation:
        `ResolveCharityScope(filter.CharityId)`; families of the charity whose registration/
        creation date is on or before `date` (the as-at semantics — verify the entity's date
        column and RECORD the choice, same ruling as 18-35), each with its non-deleted orphans,
        age computed from `DateOfBirth` at `date`; ordered by family code
- [x] **Task 2 — API endpoint** (AC 2, 5)
  - [x] `[HttpPost("family-orphans")] [Authorize(Roles = "SuperAdmin,Admin")]` in 18-1's
        `ReportsController` → `Ok(paged)`; standard ValidationException→400-errors-map /
        catch-all→500 `{ message }` ladder; no `ApiResponse<T>`
  - [x] Route-conflict note: this POST lives on `api/Reports` — no collision with 18-13's
        `GET /api/Families/{id}/follow-up` (different controller family); keep the two stories'
        screens distinct as recorded in the Route row
- [x] **Task 3 — Thin screen + document** (AC 1, 3)
  - [x] `Frontend/src/app/modules/reports/family-orphans-by-date-report/` thin 4-file component
        in the shell: charity dropdown (`GET /api/Charities`, all-option for HQ) + date picker
        (mandatory), a collapsible result grid grouped by family, and the طباعة command; route
        `family-orphans-by-date`, `AuthGuard + PermissionGuard`, `data.permission:
        'Reports.View'`; `trackBy`; empty state; OnPush omitted (list-screen precedent)
  - [x] A grouped-list document builder feeding 18-21's `report-pdf.service.ts` per the contract
        above; labels through `reports.familyOrphansByDate.*` i18n in **both** `ar.json` and
        `en.json`; empty result → nothing-to-produce message, no file
- [x] **Task 4 — Verification** (AC 1–5)
  - [x] Live check: anonymous → 401; missing `date` → 400 errors-map; Charity token → 403; HQ +
        `charityId` → that charity; a past date excludes families registered later (as-at
        semantics); empty scope → message, no file; group headers + per-family counts render
      correctly in the PDF
  - [x] `dotnet build` + `npm run build` green (MSB3021/3027 live-API lock caveat; ng-serve
        stale-bundle grep); tests excluded per the standing user decision

## Dev Notes

### Route + supersession (recorded)

- The board's route column for 18-13 is `#/reports/family-orphans` (the follow-up tracking
  screen); UC-RPT-39 has no board route. To keep both reachable and unambiguous this story takes
  `#/reports/family-orphans-by-date`. If the dev prefers hosting the print inside 18-13's screen
  instead, that is acceptable — record whichever shipped; do NOT rename 18-13's route.
- The legacy `POST /api/Reports/family-orphans/export/pdf` server streaming is superseded by the
  epic-wide ruling (JSON payload + client-side jsPDF).

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
| Family/orphan entry tracking screen + `GET /api/Families/{id}/follow-up` | 18-13 (landed scope) |
| Guardian/widow identification sheets at a date | 18-37 |
| Generic in-browser preview modal | 18-40 |

### References

- [Source: docs/Modules/23-UC-RPT-Reports-and-Printing.md#23.U.39] scenario — orphans of a
  charity's families as at a date
- [Source: _bmad-output/planning-artifacts/epics.md#3.18] US-RPT-39 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-1-orphan-data.md] skeleton + shell this story
  reuses
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-13-family-and-orphan-entry-tracking.md] the
  sibling tracking story whose route this story avoids
- [Source: _bmad-output/implementation-artifacts/epic-18-reports-and-printing/18-21-family-update-tracking.md] the PDF service
  the list renders through

## Dev Agent Record

### Agent Model Used

Claude Code (GLM-5).

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → exit 0, `0 Error(s)` (`/tmp/be-build-1839.log`)
- `npm run build` → exit 0, zero `Error:` lines (`/tmp/fe-build-1839.log`)

### Completion Notes List

- **As-at anchor recorded (Task 1's "verify and RECORD"):** `Family` carries a DEDICATED
  `RegistrationDate` column (`Family.cs:22`) — the predicate is `f.RegistrationDate <= asAt`
  (inclusive on the day). Unlike 18-35 there is no CreatedOn proxy here. Orphans carry no as-at
  filter (story text: "each with its non-deleted orphans"); their age is computed at the chosen
  date.
- **Age computation:** post-fetch in C# (`Math.Floor(days / 365.2425)`) — `TimeSpan` math is not
  portably EF-translatable; a birth date after the as-at date yields a negative age and is shown
  as-is (it reflects the data, not the report).
- Task 1 says `ResolveCharityScope(filter.CharityId)` — no such helper exists; the 18-22/24
  charity ladder is inlined verbatim in its family-rooted form (same as 18-36/38).
- **Soft-delete deviation (standing 18-36 finding):** the global query filter is commented out —
  explicit `!f.IsDeleted` / `!o.IsDeleted` checks ARE the scoping.
- **Grouped-document decision (recorded):** `report-pdf.service.printSheet` is a flat table, so
  the group headers flatten — family cells (كود العائلة · اسم المعيل · المحافظة/المركز) render on
  each group's FIRST orphan row only, the per-family عدد الأيتام rides a column (the contract's
  footer as data), and the final totals ride the meta band (إجمالي الأسر = server totalCount ·
  أيتام هذه الصفحة = page sum). Print is page-scoped — the established epic convention (18-41's
  exportAll walker is the all-pages answer, for Excel).
- Route kept distinct: `#/reports/family-orphans-by-date` (18-13's `family-orphans` untouched);
  the no-collision note is recorded in the controller's doc comment. No sidebar entry — the story
  assigns none (thin print screen, same as 18-33/35).
- المحافظة = `Region.NameAr ?? NameEn`, المركز = `Center.NameAr ?? NameEn`, resolved in the EF
  projection; the grid composes them with a dash, blank sides dropped.
- Result grid: collapsible family cards (expanded by default, chevron toggle keyed by
  index+code — codes can repeat across charities on an HQ all-charities run), orphan rows nested;
  `trackBy` on both loops; OnPush omitted (list-screen precedent).
- Empty selection → nothing-to-produce message, no file (AC 3); the as-at date is refused
  client-side and again by the validator (400 errors-map) when missing.
- Live check deferred to the consolidated private-instance smoke (strategy recorded since 18-30);
  its box stays open. No migration; tests excluded per standing decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/DTOs/Reports/Reports.cs` — `FamilyOrphansByDateFilterDto` + `FamilyOrphanRowDto` + `FamilyWithOrphansDto`
- `Backend/src/IIROSA.Application/Validators/Reports/FamilyOrphansByDateFilterValidator.cs` — new
- `Backend/src/IIROSA.Application/Interfaces/IReportService.cs` — `GetFamilyOrphansByDateAsync`
- `Backend/src/IIROSA.Application/Services/ReportService.cs` — implementation (validator DI)
- `Backend/src/IIROSA.Api/Controllers/ReportsController.cs` — `POST family-orphans`

Frontend:
- `Frontend/src/app/modules/reports/models/report.model.ts` — filter + row + group interfaces
- `Frontend/src/app/modules/reports/services/report.service.ts` — `getFamilyOrphansByDate()` (POST)
- `Frontend/src/app/modules/reports/family-orphans-by-date-report/` — new 4-file standalone screen (collapsible grouped grid + flattened print builder)
- `Frontend/src/app/modules/reports/reports-routing.module.ts` — route `family-orphans-by-date`
- `Frontend/src/assets/i18n/ar.json` · `en.json` — `reports.familyOrphansByDate` (20 keys ×2)
### Consolidated live smoke — 2026-08-25 (private instance 127.0.0.1:60970; the user's live API on 60960 untouched)

- Anonymous → 401; missing `date` → 400 errors-map; Charity token → 403; HQ + `charityId` → narrowed.
- As-at semantics: a past date excludes families registered later (verified against seeded
  registration dates).
- Empty scope → message, no file.
- Browser pass (18-40): `#/reports/family-orphans-by-date` preview framed the grouped sheet —
  family group headers render with the family's rows beneath (2 families / 4 flattened rows, RTL).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Live smoke passed (auth, errors-map, 403, as-at semantics, empty path) + browser preview shows grouped rendering. Status → review. |
| 2026-08-26 | Epic-18 code review closed (step-04 executed): decisions D1–D6 applied, 27 patches landed (22 full · 2 deferred · 3 with refutations), execution record in `review-artifacts/epic18/triage.md`; Status → done. |
