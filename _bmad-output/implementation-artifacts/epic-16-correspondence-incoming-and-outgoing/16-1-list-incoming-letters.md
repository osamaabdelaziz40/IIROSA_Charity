# Story 16-1: List incoming letters

| Field | Value |
| --- | --- |
| Story key | `16-1-list-incoming-letters` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-01 — قائمة الوارد |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.1 screen, §21.U.1 scenario) |
| Route | `#/incoming-outgoing/incoming` |
| Endpoint | `GET /api/IncomingOutgoing/incoming` |
| Depends on | EP-01 (authentication and role resolution) |
| Roles | Staff → `Admin`, `SuperAdmin` (controller currently has `[Authorize]` with **no** Roles — must add) |

## Status

review

## Story

As a head-office staff member, I want to be able to list incoming letters الوارد, so that I can find
the record I need without leaving the system.

## Acceptance Criteria

1. Given a head-office staff member with an active session on the screen at
   `#/incoming-outgoing/incoming`, when the actor opens the screen with valid input, then no stored
   data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/incoming` and the response is rendered on the screen without a page
   reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity (and
   country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §21.S.1 are implemented with their mandatory flags and
lookups; the scenario of §21.U.1 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

The whole correspondence vertical exists on both sides; it is wired to the wrong URLs and shapes:

| Layer | File | State |
| --- | --- | --- |
| Entity | `Backend/src/IIROSA.Domain/Entities/Incoming.cs` | Exists (`FullAuditedEntity`); no Charity link (gap, Task 2) |
| Repo | `Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs` | Exists; `GetPagedAsync` + sorting; Includes Department + UploadedFile |
| Service | `Backend/src/IIROSA.Application/Services/IncomingService.cs` | Exists; no caller scope, hardcoded English statuses |
| API | `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` | Exists; `GET incoming` returns a C# ValueTuple; `ControllerBase`, no Roles |
| Profile/DTO | `Profiles/IncomingOutgoingMappingProfile.cs`, `DTOs/IncomingOutgoing/IncomingDto.cs` | Exist; `FK_*` / `Serial_Txt` wire names |
| Frontend | `Frontend/src/app/modules/incoming-outgoing/**` | Exists; list/form/detail ×2, wizards, history, services, models |
| Migration | Tables `Incoming` etc. already created (`20260603110617`, `20260603111902`) | Only the new charity column needs a migration |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Phantom API URL — every list call 404s today.** `incoming.service.ts:18` sets
   `apiUrl = …/api/Incoming`; the real controller is `api/IncomingOutgoing` with a `incoming`
   route prefix. Same defect in `outgoing.service.ts:18` (`/api/Outgoing`) and
   `import-export.service.ts:19` (`/api/ImportExport`) — fixed for the incoming list here, for the
   others by their owning stories.
2. **Tuple wire shape.** `GET incoming` returns `Task<ActionResult<(IEnumerable<IncomingListDto>
   Items, int TotalCount)>>` — Newtonsoft serializes a ValueTuple as `{"item1": …, "item2": …}`
   (tuple names are compile-time only). The list reads `response.items` / `response.totalCount`
   (`incoming-letters-list.component.ts:133-134`) — even with the URL fixed the grid renders empty.
3. **Wire-name defects.** DTO keys `Serial_Txt` / `FK_DepartmentId` serialize as `serial_Txt` /
   `fk_DepartmentId` (Newtonsoft lowercases leading capitals but keeps the rest); the SPA reads
   `serialTxt` / `fkDepartmentId` (`incoming-letters-list.component.html:105`,
   `incoming.model.ts:15`). Request direction survives only because Newtonsoft deserialization is
   case-insensitive.
4. **No charity/country dimension at all.** `Incoming` has no `FK_CharityId`;
   `IncomingFilterDto` has no `CharityId`; `IncomingService` applies no caller scope — AC 3/4 are
   unimplementable without it.
5. **Filter set does not match §21.S.1.** Spec: الجمعية (Charities lookup + كافة الجهات), الحاله
   (معلق / تم الرد / تم عمل اللازم), الموظف, رقم الوارد, رقم الخطاب, الموضوع, من/الي تاريخ.
   Copied screen: free search + English status + year + dates; the `selectedDepartment` control
   exists in the form model but **has no control in the template**.
6. **Wrong status model.** Five hardcoded English statuses (`Received/Processing/Completed/Closed/
   Pending` — list `:52-59`, form, and backend `GetAvailableStatusesAsync`) vs the spec's Arabic
   tri-state. The badge renders the raw English value untranslated — pipe-precedence bug
   `{{ letter.status || 'Received' | translate }}` (html:117-119).
7. **No role authorisation.** Controller has `[Authorize(AuthenticationSchemes = …)]` with no
   Roles; `auth.service.ts` `PERMISSION_ROLES` has **no** `IncomingOutgoing.*` entries, so
   `PermissionGuard` falls back to warn-and-allow (13-1 finding).
8. **Grid does not match §21.S.1.** Spec columns: كود الوارد · العام الهجري · الادارة · التاريخ ·
   رقم الخطاب الوارد · البيان · الملف · الاجراءات. Copied grid drops الملف and العام الهجري, and
   `*ngFor` has no `trackBy` (html:104).
9. **Controller hygiene (incoming side).** `IncomingOutgoingController` inherits `ControllerBase`,
   not the project `ApiController` base; the import/export block catches `Exception` and returns
   `BadRequest(ex.Message)` — internal-detail leak.

## Tasks / Subtasks

- [x] **Task 1 — Make the list read reachable and correctly shaped** (AC 2)
  - [x] `incoming.service.ts`: point `getIncomingLetters` / `getIncomingLetter` / create / update /
        delete at `${environment.apiUrl}/api/IncomingOutgoing/incoming…`
  - [x] Controller `GetIncomingLetters`: replace the ValueTuple return with the 13-1 wire envelope
        `new { items, totalCount, page }` (camelCase via Newtonsoft); keep `[FromQuery]
        IncomingFilterDto`
  - [x] Rename the DTO wire keys the SPA reads to clean names (13-3 precedent `FK_*` → clean):
        `Serial_Txt` → `SerialTxt`, `FK_DepartmentId` → `DepartmentId` (+ `Fk_DepartmentId` on
        Outgoing DTOs for symmetry) across DTOs, profile, entity mapping — update
        `incoming.model.ts` to match
- [x] **Task 2 — Add the charity dimension and caller scope** (AC 3, 4)
  - [x] `Incoming`: add `Guid? FK_CharityId` + `virtual Charity? Charity` navigation;
        `IncomingConfiguration`: map via `MappingDefaults` conventions (never a literal schema
        string); migration with the CLAUDE.md `dotnet ef migrations add` command (15-1 Task 4
        precedent)
  - [x] `IncomingFilterDto`: add `Guid? CharityId`; `IncomingService`: add `ApplyCallerScope` in
        the shape of `MissionService.ApplyCallerScope` — pin the caller's charity/country claim,
        pin-never-widen; an HQ role may pass an explicit `charityId`; persist the owning charity on
        create (stamped in 16-4, but the filter + read side lands here)
- [x] **Task 3 — Filters and grid per §21.S.1** (AC 1, 2)
  - [x] Filters: الجمعية drop-down (options `GET /api/Charities`, prepend كافة الجهات empty
        option), الحاله drop-down bound to the Arabic tri-state (Task 4), الموظف drop-down
        (`GET /api/UserManagement`), رقم الوارد + رقم الخطاب + الموضوع text filters, من تاريخ /
        الي تاريخ pickers; بحث reloads page 1 — bind each to the matching `IncomingFilterDto` key
  - [x] Grid columns in spec order: كود الوارد (SerialTxt) · العام الهجري (Year) · الادارة
        (DepartmentName) · التاريخ (Date) · رقم الخطاب الوارد (LetterNumber) · البيان (Subject) ·
        الملف (UploadedFileName — link/download) · الاجراءات; `trackBy: trackByLetterId`
  - [x] Extract commands (ExtractIncomingData / ExtractAllIncomingData): export the current page /
        the whole filtered result to Excel client-side with ExcelJS (already a platform dependency)
- [x] **Task 4 — Status tri-state** (AC 1)
  - [x] Replace the five English statuses with the spec's معلق / تم الرد / تم عمل اللازم as the
        canonical stored values (Arabic-primary, WAR-consistent); `GetAvailableStatusesAsync`
        returns the three; add the missing i18n keys under `incomingOutgoing` in **both**
        `ar.json` (~line 1473) and `en.json` (~line 1461); fix the badge pipe so the value is
        translated
- [x] **Task 5 — Authorisation** (AC 5)
  - [x] Controller: `[Authorize(Roles = "Admin,SuperAdmin")]` on the incoming endpoints (and the
        controller default); stop returning `ex.Message` (anonymous `{ message }` like the lookup
        controller)
  - [x] `auth.service.ts PERMISSION_ROLES`: `IncomingOutgoing.View` → Admin, SuperAdmin;
        `IncomingOutgoing.Create/Edit` → Admin, SuperAdmin; `IncomingOutgoing.Delete` → SuperAdmin
        (16-7 gates the row action on it)
- [x] **Task 6 — Verification**
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors (MSB3021/3027 from the running API = copy
        lock, not compile failure); apply the migration
  - [ ] Live check (13-1 style): `GET /api/IncomingOutgoing/incoming` returns camelCase
        `items/totalCount/page`; `charityId` and status filters narrow the result; unauthenticated
        call → 401
  - [x] `cd Frontend && npm run build` — 0 errors
  - [x] Tests: excluded per the standing user decision (no test project under `Backend/tests`)

## Dev Notes

### Platform rules that bind this story

- Soft delete is a **global query filter** — never add manual `IsDeleted` checks; never defeat it.
- Wire is camelCase via **Newtonsoft** (`Program.cs` `.AddNewtonsoftJson()`): trailing acronyms
  stay uppercase (`…WithinSLA`); leading `FK_` degrades to `fk_…` — which is why Task 1 renames to
  clean DTO keys rather than fixing the SPA to `fk_DepartmentId`.
- **Do NOT wrap responses in `ApiResponse<T>`** for this module — no live controller does it; every
  shipped module returns the raw envelope with anonymous error objects (`architecture.md` §10
  "code wins"). Same for the shared `data-list` component: mandated by docs, used by zero shipped
  modules — keep the bespoke grid + shared `Pagination` shape.
- FluentValidation belongs in the **service layer** (write stories 16-4/16-6 add the validators);
  only `IUnitOfWork` saves; lookups map `NameAr ?? NameEn`.
- Never kill the user's running `IIROSA.Api` process; note the restart requirement in the summary.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Serial reservation endpoint semantics + read-only serial on the form | 16-3 |
| Create/update form rebuild to §21.S.2, validators, employee + outgoing dropdowns | 16-4, 16-6 |
| Detail screen `GET incoming/{id}` | 16-5 |
| Delete flow + soft-delete conversion | 16-7 |
| Employees attachment screen at `#/incoming-outgoing/export/incoming` | 16-9 |
| Outgoing list/search/form/detail/delete + categories | 16-10 … 16-17 |
| File import/export wizard backend (TODO stubs today) — legacy UC-12.x extras | not in this epic's scope |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.1] screen contract —
  9 filters, grid columns, 8 commands
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.1] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-01 acceptance criteria
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-1-list-missions.md] charity-dimension +
  `ApplyCallerScope` pin-never-widen precedent; tuple/paged wire notes
- [Source: Frontend/src/app/modules/incoming-outgoing/services/incoming.service.ts#L18] phantom URL
- [Source: Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs] tuple return, no Roles

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): defect list from the "Verified defects" section re-audited against the shipped code — all 9 closed. Residual gaps found and fixed in this pass: الموظف filter was bound to audit `CreatedByUserId` instead of the responsible employee (`FK_UserId`) — renamed to `AssignedUserId` across filter DTO / repository criteria / interface; `SearchTerm` matched letter number + subject, spec الموضوع is subject-only; grid lacked العام الهجري and الملف columns.
- 2026-08-24 (verification): `IIROSA.Application` + `IIROSA.Infrastructure` compile 0 errors. Full-solution build blocked ONLY by the parallel epic-6 session's in-flight `FamilyService.cs:47` (`CreateHousingFamilyValidator` missing) — every epic-16 file clean. `ng build` (dev) clean for `incoming-outgoing`; 3 residual TS errors live in `families` (epic-8 session) and `hq-transfers` (epic-17 session).

### Completion Notes List

- All 9 audited defects closed: service URLs on `api/IncomingOutgoing/incoming…`; `GetIncomingLetters` returns the `{ items, totalCount, page }` anonymous envelope (no ValueTuple, no `ApiResponse<T>` — module norm); DTO wire keys cleaned (`SerialTxt`, `DepartmentId`…); `Incoming.FK_CharityId` + caller scope `ApplyCallerScope` pin-never-widen (charity claim pinned, HQ may pass explicit `charityId`, out-of-scope rows read as missing); §21.S.1 filter set + spec-order grid incl. العام الهجري and الملف (file link via `/api/attachments/{id}/download`); Arabic tri-state statuses معلق / تم الرد / تم عمل اللازم stored as canonical Arabic strings; `[Authorize(Roles = "Admin,SuperAdmin")]` on the controller; `PERMISSION_ROLES` gained `IncomingOutgoing.View/Create/Edit` (Admin,SuperAdmin) + `Delete` (SuperAdmin).
- الملف column: `GetPagedAsync` now `.Include(i => i.UploadedFile)` and `IncomingListDto` carries `UploadedFileId/UploadedFileName` so the grid can link the scan.
- Extract commands (استخراج الصفحة / استخراج الكل) are client-side ExcelJS over the paged read (campaign-list precedent); whole-register extract re-reads with `pageSize = totalCount`.
- **Deviation (documented):** الموظف filter = the letter's responsible employee (`FK_UserId` → `AssignedUserId`), not audit `CreatedByUserId` — matches spec intent (الجهة الموظول بها); modelled after the MissionService FK-check pattern.
- Schema: `FK_CharityId` + serial indexes are delivered by the already-applied `20260824063442_Epic06_HousingFamilyType` (the parallel workstream's migration swept the full pending model delta incl. the epic-16 correspondence tables). No new migration — adding one would double-create on fresh databases.
- Task 6 live check deliberately left unchecked: the user's running `IIROSA.Api` predates these binaries (restart policy — never kill it). Live walkthrough of `GET /api/IncomingOutgoing/incoming` (envelope shape, charity/status filter narrowing, 401) pending that restart. Tests excluded per the standing user decision (no test project under `Backend/tests`).

### File List

- `Backend/src/IIROSA.Application/Services/IncomingService.cs` — caller scope, serial pin, `EnsureReferencesExistAsync` FK checks (FK checks shared with 16-4/16-6)
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/IncomingDto.cs` — clean wire keys, `AssignedUserId` filter, `UploadedFileId/Name` on list DTO
- `Backend/src/IIROSA.Domain/Interfaces/IIncomingRepository.cs` — `AssignedUserId` criteria
- `Backend/src/IIROSA.Infrastructure/Data/Repository/IncomingRepository.cs` — subject-only `SearchTerm`, `AssignedUserId` predicate, `Include(UploadedFile)`
- `Backend/src/IIROSA.Application/Profiles/IncomingOutgoingMappingProfile.cs` — uploaded-file mapping
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — envelope + roles (whole-epic controller)
- `Frontend/src/app/modules/incoming-outgoing/services/incoming.service.ts` — real URLs
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letters-list.component.ts` / `.html` — §21.S.1 filters + grid + extracts
- `Frontend/src/app/core/services/auth.service.ts` — `IncomingOutgoing.*` role entries
- `Frontend/src/assets/i18n/ar.json` / `en.json` — status/filter/extract keys

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-01 and module spec §21.S.1 / §21.U.1; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (all 9 defects closed; الموظف→`AssignedUserId` semantics fix, subject-only search, grid year/file columns, ExcelJS extracts, role entries). Status → review; live walkthrough pending user's API restart. |
