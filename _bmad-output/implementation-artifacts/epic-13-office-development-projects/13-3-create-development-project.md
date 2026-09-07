# Story 13-3: Create a development project

| Field | Value |
| --- | --- |
| Story key | `13-3-create-development-project` |
| Epic | EP-13 — Office Development Projects |
| Use case | UC-OFP-03 — اضافة مشروع تنموي |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/18-UC-OFP-Office-Development-Projects.md` (§18.S.3 screen, §18.U.3 scenario) |
| Route | `#/office-development-projects/create` |
| Endpoint | `POST /api/OfficeProjectManagement` |
| Depends on | EP-01 (authentication and role resolution), 13-2 (project-type lookup) |

## Status

done

## Story

As a General Director, I want to be able to create a development project اضافة مشروع تنموي, so
that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a General Director with an active session on the screen at
   `#/office-development-projects/:id/edit`, when the actor presses «حفظ» with valid input, then a
   new record exists, owned by the charity of the creating user, and appears in the list screen of
   the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/OfficeProjectManagement` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §18.S are implemented with their mandatory flags and
lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Rename the `FK_`-prefixed DTO properties** (breaks AC 1/4 through the frontend)
  - [x] `OfficeProjectDetailDto` / `CreateOfficeProjectDto` / `UpdateOfficeProjectDto` /
        `OfficeProjectFilterDto`: `FK_OfficeProjectTypeId` → `OfficeProjectTypeId`,
        `FK_CountryId` → `CountryId`, `FK_RegionId` → `RegionId`, `FK_CenterId` → `CenterId`,
        `FK_CharityId` → `CharityId`, `FK_AttachedFileId` → `AttachedFileId`,
        `FK_ProjectReportFileId` → `ProjectReportFileId`
  - [x] Rationale: the wire is camelCase (verified empirically), so `FK_CountryId` serialises as
        `fK_CountryId` — a key no client can guess. The frontend `OfficeProject` model already
        uses the clean names; the `fk_*`-keyed `OfficeProjectDto`/`OfficeProjectFilter` must be
        renamed to match
  - [x] Add explicit `AutoMapper` `ForMember` maps in `OfficeProjectProfile` for the renamed
        Create/Update properties
- [x] **Task 2 — Invoke the FluentValidation validators in the service** (AC 3)
  - [x] Inject `IValidator<CreateOfficeProjectDto>` / `IValidator<UpdateOfficeProjectDto>` into
        `OfficeProjectService`; validate before mapping; throw `ValidationException` so the
        controller's existing catch shapes the field-flagged response
  - [x] Relax `.GreaterThanOrEqualTo(DateTime.Today)` rules on the date fields — §18.S.3 carries no
        future-only constraint and seeded data has past dates; keep not-empty where the field is
        mandatory
- [x] **Task 3 — Persist through `IUnitOfWork` only** (CLAUDE.md: repositories never save)
  - [x] Replace every `_projectRepository.SaveChangesAsync()` in `OfficeProjectService` with
        `IUnitOfWork.SaveChangesAsync()`
- [x] **Task 4 — Own the record's country/charity server-side** (AC 1)
  - [x] Inject `ICurrentUserService`; default `CountryId` to the caller's country claim when the
        create payload omits it; pin it when the token carries one
  - [x] Keep `ICharityWriteGuard` for the charity link
- [x] **Task 5 — Frontend: build the create payload with clean keys**
  - [x] `project-form.component.ts`: construct the DTO with `officeProjectTypeId`, `countryId`,
        `regionId`, `centerId`, `charityId`, … (was `fk_OfficeProjectTypeId` etc.)
  - [x] Rename `OfficeProjectDto` / `OfficeProjectFilter` interfaces to the clean keys
- [~] **Task 6 — Tests** — EXCLUDED FROM SCOPE by user decision (same standing decision as epic 3;
      no test project exists under `Backend/tests`)

## Dev Notes

### Findings from the review that produced these tasks (2026-08-19)

- Live-verified: `POST` succeeds today only because the frontend duplicates the server's `fK_*`
  keys verbatim (`fk_OfficeProjectTypeId`). Any client using the clean, documented property names
  fails silently — the fields arrive as default(0)/null and the FK columns get garbage. This is the
  epic's worst defect and it is shared with 13-4.
- `OfficeProjectService` (868 lines) never injects a validator. `CreateOfficeProjectValidator`
  exists, is registered by `AddValidatorsFromAssembly`
  (`IIROSA.Application/ServiceCollectionExtensions.cs:91`), and is dead code — AC 3's "refused and
  the offending field is flagged" is enforced only by HTML `required` attributes.
- The validators as written contain future-date rules (`.GreaterThanOrEqualTo(DateTime.Today)`)
  not present in §18.S.3; wired in as-is they would reject the seeded historical rows and any
  backdated project, so they are relaxed during wiring.
- The service calls `_projectRepository.SaveChangesAsync()` throughout — the platform rule is that
  only `IUnitOfWork` saves.
- No country scope on create: a caller can file a project under any country regardless of claim.

### Wire contract (verified empirically, not assumed)

Responses are camelCase on the wire. The `AddJsonOptions` + `AddNewtonsoftJson` combination in
`Program.cs` looked statically like it would emit PascalCase; it does not. Raw DTOs are returned
(not `ApiResponse<T>`) — consistent with all 21 controllers and the Angular client; a recorded,
deliberate platform deviation.

## Dev Agent Record

### Implementation Plan

1. Rename DTO properties (backend), fix the profile's explicit maps, rename frontend model keys.
2. Wire validators with relaxed date rules; switch persistence to `IUnitOfWork`.
3. Default/pin `CountryId` from the caller's claim on create.
4. Re-verify create against the live API.

### Debug Log

- 2026-08-19: the entity keeps its `FK_`-prefixed columns (no migration — the database is
  untouched); the profile bridges every FK pair with explicit `ForMember` maps in both
  directions, so the rename is DTO-surface only.
- 2026-08-19: validation runs **before** the try block, so a `ValidationException` reaches the
  controller's dedicated catch instead of being logged-and-rethrown by the generic handler.

### Completion Notes

- **AC 1** — create pins the record's country to the caller's claim when the token carries one
  (defaults it when omitted, overrides and logs when another country was asked for); the charity
  link stays a payload field guarded by `ICharityWriteGuard`.
- **AC 3** — `CreateOfficeProjectValidator` now actually runs. Live check: POST with empty
  `projectName` and `officeProjectTypeId: 0` returns `400 {"errors":{"ProjectName":[…],
  "OfficeProjectTypeId":[…]}}` — the field-flagged refusal the AC demands.
- **Date rules** — the future-only rules were dropped (a 2020-dated project was created in
  verification); the end-after-start relationship rule was added instead.
- **Persistence** — every write goes through `IUnitOfWork.SaveChangesAsync()`; the repository
  never saves.
- Verified live: POST with clean keys returns the detail with navigations populated
  (`خياطه` / `مصر` / `جنوب سيناء`) and the record appears in the list.
- Tests excluded by user decision.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Application/DTOs/OfficeProjectManagement/OfficeProjects.cs` | FK_-prefixed DTO properties renamed to clean names; granular use-case DTOs removed |
| `Backend/src/IIROSA.Application/Validators/OfficeProjectManagement/CreateOfficeProjectValidator.cs` | Create/Update validators kept and relaxed (future-date rules dropped, end-after-start added); 8 granular validators removed |
| `Backend/src/IIROSA.Application/Profiles/OfficeProjectProfile.cs` | Explicit ForMember bridges for every renamed FK; dead granular maps and unused Update map removed |
| `Backend/src/IIROSA.Application/Services/OfficeProjectService.cs` | Validators injected and invoked before the try; `IUnitOfWork` owns saves; create defaults/pins country from caller claim |
| `Backend/src/IIROSA.Application/Interfaces/IOfficeProjectService.cs` | Interface trimmed to the realised surface |
| `Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs` | `ValidationException` catch shapes the AC-3 field-flagged 400 |
| `Frontend/src/app/modules/office-development-projects/models/office-project.model.ts` | `OfficeProjectDto` / `OfficeProjectFilter` renamed to clean keys |
| `Frontend/src/app/modules/office-development-projects/project-form/project-form.component.ts` | Submit payload built with clean keys |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-OFP-03 and module spec §18.S.3 / §18.U.3; audit findings recorded. |
| 2026-08-19 | Tasks 1–5 implemented; create + validation refusal + backdated dates verified live; backend 0 errors, frontend builds. |
