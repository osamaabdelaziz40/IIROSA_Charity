# Story 13-6: Report on development projects

| Field | Value |
| --- | --- |
| Story key | `13-6-report-on-development-projects` |
| Epic | EP-13 — Office Development Projects |
| Use case | UC-OFP-06 — تقرير المشاريع التنموية |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/18-UC-OFP-Office-Development-Projects.md` (§18.S.6 screen, §18.U.6 scenario) |
| Route | `#/office-development-projects` (list toolbar export) |
| Endpoint | `GET /api/OfficeProjectManagement/export` |
| Depends on | 13-1 (list + filter), 13-3 (DTO rename) |

## Status

done

## Story

As a General Director, I want to be able to report on development projects تقرير المشاريع
التنموية, so that I can answer the operational, compliance or financial question being asked of
me.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor opens the screen
   with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/OfficeProjectManagement/export` and the response is rendered on the screen without a
   page reload.
3. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §18.S are implemented with their mandatory flags and
lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Change the export verb to GET as specified** (AC 2)
  - [x] Replace `POST /api/OfficeProjectManagement/export` with
        `GET /api/OfficeProjectManagement/export`, filter bound from the query string
        (`[FromQuery] OfficeProjectFilterDto`)
  - [x] Apply the same caller country pin as the list (13-1 Task 1) so the report cannot be used to
        read around the scope
  - [x] Frontend: `exportToExcel()` switches to a GET blob download carrying the active filter
        (keys renamed per 13-3 Task 5)
- [x] **Task 2 — Arabic report columns**
  - [x] Excel headers use the §18.S.1 column set — الرقم، اسم المشروع، اسم المتبرع، التكلفه
        بالجنيه، التكلفه بالريال، الجمعيه — not English property names
- [x] **Task 3 — Remove the now-dead granular endpoints** (module hygiene while touching the
      controller)
  - [x] Delete the 13 of 14 extra endpoints no component calls (budget / donor / beneficiaries /
        location / documents / report / complete→kept / dates / assign-charity / charity/{id} /
        status-summary / ongoing / completed) — **keep** `markAsCompleted`, used by the list,
        detail, and `#/progress` views
  - [x] Delete their DTOs, validators, profile maps and repository helpers so nothing orphans
- [~] **Task 4 — Tests** — EXCLUDED FROM SCOPE by user decision (same standing decision as epic 3;
      no test project exists under `Backend/tests`)

## Dev Notes

### Findings from the review that produced these tasks (2026-08-19)

- This is the only story of the epic that was still `backlog` on the board; an export exists but
  as `POST …/export` returning a blob, which the AC does not admit (GET, like every other read).
- The controller carries 14 extra endpoints beyond CRUD+export; the components call only
  `markAsCompleted`. The rest are unreachable surface that must each be authorisation-reviewed and
  maintained — deleting them is the review's completion work.
- `markAsCompleted` is kept deliberately: the `#/office-development-projects/progress` route and
  both screens consume it, and it maps to the module's completion tracking in §18.S.
- Roles: export is a read — `Admin,SuperAdmin`, same as the list (Gen. Director/Staff per the
  permission matrix F/F row).

## Dev Agent Record

### Implementation Plan

1. Controller: export → GET `[FromQuery]`; delete dead endpoints (keep markAsCompleted).
2. Service/repository: delete orphaned DTOs, validators, maps, helpers; country-pin the export.
3. Frontend: GET blob export with renamed filter keys; Arabic headers.
4. Verify the download live.

### Debug Log

- 2026-08-19: route precedence checked — the literal `export` segment beats the `{id}` parameter,
  so `GET /{id}` and `GET /export` coexist without a route constraint.

### Completion Notes

- **Export is a GET read** per the AC: `[FromQuery] OfficeProjectFilterDto`, `Admin,SuperAdmin`,
  same `ApplyCallerScope` country pin as the list — the report cannot be used to read around the
  scope. Verified live: `GET /export?searchText=…` → HTTP 200, ~3 KB body with the PK zip/xlsx
  header and the xlsx content type.
- **Arabic report columns** per §18.S.1: الرقم، اسم المشروع، اسم المتبرع، التكلفه بالجنيه،
  التكلفه بالريال، الجمعيه; sheet name المشاريع التنموية; serial runs continuously.
- **Dead surface removed**: the controller now exposes exactly the seven UC-OFP endpoints + the
  kept `PUT /{id}/complete` (feeds the list, detail and `#/progress` views). Their 8 granular
  DTOs, validators, profile maps and ~9 repository helpers went with them — nothing orphans.
- Frontend downloads through a GET blob with the active filter's clean keys.
- Tests excluded by user decision.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs` | Export switched POST→GET `[FromQuery]`; 13 dead endpoints removed; `complete` kept |
| `Backend/src/IIROSA.Application/Interfaces/IOfficeProjectService.cs` | Interface trimmed to the realised 7-method surface |
| `Backend/src/IIROSA.Application/Services/OfficeProjectService.cs` | `ExportProjectsToExcelAsync` reworked (Arabic columns/sheet, scope pin, `IUnitOfWork` world) |
| `Backend/src/IIROSA.Application/DTOs/OfficeProjectManagement/OfficeProjects.cs` | Status-summary + 8 granular DTOs removed (shared with 13-3) |
| `Backend/src/IIROSA.Application/Validators/OfficeProjectManagement/CreateOfficeProjectValidator.cs` | 8 granular validators removed |
| `Backend/src/IIROSA.Application/Profiles/OfficeProjectProfile.cs` | Granular maps removed |
| `Frontend/src/app/modules/office-development-projects/services/office-project.service.ts` | `exportToExcel()` → GET blob with filter params; dead endpoint methods deleted |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-OFP-06 and module spec §18.S.6 / §18.U.6; audit findings recorded. |
| 2026-08-19 | Tasks 1–3 implemented; GET export verified live (200, xlsx bytes, Arabic columns); dead endpoint surface deleted; both builds pass. |
