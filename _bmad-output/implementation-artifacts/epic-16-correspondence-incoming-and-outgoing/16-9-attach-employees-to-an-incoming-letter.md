# Story 16-9: Attach employees to an incoming letter

| Field | Value |
| --- | --- |
| Story key | `16-9-attach-employees-to-an-incoming-letter` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-09 — ربط الموظفين بخطاب وارد |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.3 screen, §21.U.9 scenario) |
| Route | `#/incoming-outgoing/export/incoming` |
| Endpoints | `GET /api/IncomingOutgoing/incoming` (letters) · `GET /api/IncomingOutgoing/incoming/{id}/employees` + `POST …/employees` + `DELETE …/employees/{userId}` (new — see defect 1) |
| Depends on | 16-4 (letters exist to attach to), 16-1 (scope + roles) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to attach employees to an incoming letter ربط
الموظفين بخطاب وارد, so that the register records who a received letter concerns.

## Acceptance Criteria

1. Given a head-office staff member with an active session on the screen at
   `#/incoming-outgoing/export/incoming`, when the actor opens the screen with valid input, then
   no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page
   reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted,
   then it is refused with that message and nothing is written.

**Definition of done:** the screen fields of §21.S.3 are implemented; the scenario of §21.U.9
passes end to end — attached and unattached employees list for the chosen letter + charity, the
selection saves, and the scoping is enforced server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Route | `export/:type` → `ExportWizardComponent` | Exists (route registered) |
| Employees source | `UserManagementController` (`api/UserManagement`, paged GET) | Exists |
| Incoming read | 16-1's fixed list endpoint | Exists after 16-1 |
| Attachment model to copy | none — this link is **new** | — |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Nothing exists for the actual use case.** No link entity, no endpoints, no UI. `Incoming`
   has only the single `FK_UserId`; UC-COR-09 attaches **multiple** employees per letter.
2. **The screen at this route is the wrong software.** `ExportWizardComponent` today is an
   Excel/PDF file-export wizard with `@Input() fileType` that a *routed* component never
   receives (route param `:type` is never read — `export-wizard.component.ts:22`), pointed at
   dead `/export` + `/export/pdf` endpoints (no PDF endpoint exists anywhere). §21.S.3 specifies
   an **employee-attachment** screen: خطاب الوارد drop-down + الجمعية, two grids (unattached
   employees → اضافة الى الخطاب; attached employees → حذف), حفظ. Replace the wizard for this
   route with the attachment screen (a new component is cleaner than mutating the wizard; keep
   the wizard file for removal in 16-18's sibling work — see Dev Notes).
3. **Business rule («Operation Faild»).** Attach/detach must refuse (localized refusal, nothing
   written) when: the letter is not visible under the caller's scope; a duplicate attach is
   requested; or the letter is soft-deleted. Model this as the service's guarded write path.

## Tasks / Subtasks

- [x] **Task 1 — Domain + persistence for the link** (AC 6)
  - [x] New entity `IncomingEmployee` (`FullAuditedEntity`, Guid): `Guid IncomingId`, `Guid
        UserId`, unique index on `(IncomingId, UserId)`; configuration via `MappingDefaults`
        conventions + Restrict behaviours; migration (CLAUDE.md command); register repository +
        service in `ServiceCollectionExtensions`
- [x] **Task 2 — Service + endpoints** (AC 1–4, 6)
  - [x] `IIncomingService`: `GetLetterEmployeesAsync(incomingId)` → attached + unattached lists
        (employees from the identity user set scoped to the caller's charity; verify the
        `identity.Users` mapping lesson from 15-1 before adding a user navigation — prefer a
        projection over a nav property), `AttachEmployeeAsync(incomingId, userId)`,
        `DetachEmployeeAsync(incomingId, userId)` — each verifies letter scope first; duplicates
        and out-of-scope letters → localized «Operation Faild» refusal, nothing written
  - [x] Controller: `GET incoming/{id}/employees`, `POST incoming/{id}/employees` (body
        `{ userId }`), `DELETE incoming/{id}/employees/{userId}` — `[Authorize(Roles =
        "Admin,SuperAdmin")]`, bare DTO/list returns, no `ex.Message`
- [x] **Task 3 — The §21.S.3 screen** (AC 1, 2)
  - [x] New component at `#/incoming-outgoing/export/incoming` (re-route from the wizard):
        خطاب الوارد drop-down (incoming list, caller-scoped) · الجمعية drop-down (HQ only,
        `GET /api/Charities` + كافة الجهات); on letter change load both grids
  - [x] Grids per §21.S.3: unattached — الرقم · إسم الموظف · اضافة الى الخطاب; attached —
        الرقم · إسم الموظف · حذف; `trackBy`; row commands call attach/detach and move the row
        between grids; حفظ persists any staged selection (or attach/detach applies immediately —
        follow the §21.U.9 read-then-save flow: stage locally, save on حفظ)
  - [x] i18n keys in both `ar.json`/`en.json`; no hard-coded strings
- [x] **Task 4 — Verification**
  - [ ] Live: attach → row moves grids + persists (reload shows it attached); duplicate attach →
        «Operation Faild» refusal; charity user cannot see another charity's letters; detach →
        back to unattached; unauthenticated → 401
  - [x] `dotnet build` + `npm run build` — 0 errors; migration applied; tests excluded per the
        standing decision

## Dev Notes

### Platform rules that bind this story

- Link entity is `FullAuditedEntity` (Guid) — soft-deletable like everything business; unique
  index prevents duplicate attaches at the storage layer, the service gives the friendly refusal.
- **User identity caution (15-1 lesson):** adding an `ApplicationUser` navigation to an app
  context entity convention-mapped to `dbo.ApplicationUser` (an empty duplicate) caused live
  `Invalid column` 500s. If a navigation is needed, reuse the `identity.Users` +
  `ExcludeFromMigrations()` remap; a LINQ projection onto a flat DTO avoids the problem
  entirely — prefer the projection.
- The spec's grid rows are labelled `orphan in allOrphans` with an إسم الموظف column — a WAR
  naming artefact; the use case (§21.U.9) governs: these are **employees**.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Orphan-report attachment (sibling screen) | 16-18 |
| The file-export/import wizards themselves (legacy UC-12.x extras, backend stubs) | not in this epic's scope — after 16-18 both wizard routes are re-pointed, the orphaned `import-export` service + history screen become dead code; record for a cleanup task, do not delete here |
| Responsible employee (single) on the form | 16-4 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.3] screen contract
  — 2 fields, 2 grids, 3 commands
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.9] scenario —
  attached/unattached lists, charity scope, «Operation Faild»
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-09 acceptance criteria
- [Source: Frontend/src/app/modules/incoming-outgoing/export-wizard/export-wizard.component.ts#L22] `@Input` on a routed component — never set

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified the whole chain — `IncomingEmployee` link entity (unique index `(IncomingId, UserId)`, `FullAuditedEntity`), `GET incoming/{id}/employees` (controller :198 → `GetEmployeesAsync` returning attached + available lists), `AttachEmployeeAsync` (:315) / `DetachEmployeeAsync` (:343) with letter-scope verification and «Operation Faild» refusals for out-of-scope/duplicate/soft-deleted, `POST/DELETE incoming/{id}/employees[/{userId}]` under Admin,SuperAdmin. Route `export/incoming` (:101) points at the new screen, not the wizard.
- 2026-08-24 (this pass): added the missing entry point — the incoming detail screen's page actions now route here with `?incomingId=` preselecting the letter in question (`ActivatedRoute` + `preselectFromQuery()`).
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module. Table delivered by the already-applied `20260824063442_Epic06_HousingFamilyType` migration — no separate migration (adding one would double-create on fresh databases).

### Completion Notes List

- Screen follows the §21.U.9 read-then-save flow: row commands (اضافة الى الخطاب / حذف) stage the attach/detach set locally and move the row between grids immediately; حفظ flushes the staged set through the per-link endpoints, then reloads both grids from the server. Employees resolve via LINQ projection (no ApplicationUser navigation — 15-1 lesson honoured).
- Naming deviation (cosmetic): service methods are `GetEmployeesAsync` / `AttachEmployeeAsync` / `DetachEmployeeAsync` (story suggested `GetLetterEmployeesAsync`) — same contract.
- The spec's grid variable names (`orphan in allOrphans`) are a WAR naming artefact — rows are employees, per §21.U.9.
- The dead `export-wizard` route was re-pointed, not deleted; the orphaned import-export service + history screen remain recorded for a later cleanup task (out of epic scope).
- i18n keys in both `ar.json`/`en.json`; no hard-coded strings. Task 4 live check left unchecked — pending the user's `IIROSA.Api` restart. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Domain/Entities/IncomingEmployee.cs` + `Configurations/IncomingEmployeeConfiguration.cs` — link entity
- `Backend/src/IIROSA.Application/Services/IncomingService.cs` — employees read + guarded attach/detach
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — employees endpoints
- `Frontend/src/app/modules/incoming-outgoing/incoming-employees/incoming-letter-employees.component.ts` / `.html` / `.scss` — §21.S.3 screen + query-param preselect
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letter-detail.component.ts` — entry-point action
- `Frontend/src/app/modules/incoming-outgoing/incoming-outgoing-routing.module.ts` — `export/incoming` route

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-09 and module spec §21.S.3 / §21.U.9; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified entity/endpoints/screen; added detail-screen entry point with `?incomingId=` preselect). Status → review; live attach/detach walkthrough pending user's API restart. |
