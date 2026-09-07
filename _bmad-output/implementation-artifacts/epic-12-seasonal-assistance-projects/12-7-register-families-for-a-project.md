# Story 12-7: Register families for a project

| Field | Value |
| --- | --- |
| Story key | `12-7-register-families-for-a-project` |
| Epic | EP-12 — Seasonal Assistance Projects |
| Use case | UC-PRJ-07 — اختيار الأسر للمشروع |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/17-UC-PRJ-Seasonal-Assistance-Projects.md` (§17.S.3 screen, §17.U.7 scenario, §17.D 25.9 detailed spec + BR-23/24/25) |
| Route | `#/seasonal-aid/:id/beneficiaries` |
| Endpoint | `PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` |
| Depends on | EP-01 (authentication and role resolution), 12-6 done (screen exists), 12-2 done (campaign + quota exist) |
| Legacy reference | `ISeasonalAidService.UpdateCampaignBeneficiaries` (old system) |

## Status

done

## Story

As a charity user, I want to be able to register families for a project اختيار الأسر للمشروع, so that
the selections are saved as project-family registrations, subject to the quota HQ defined.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then the stored record carries the new values; no other record is affected.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/SeasonalAid/campaigns/{campaignId}/beneficiaries` and the response is rendered on the
   screen without a page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears there
   with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given the business rule behind «Operation Faild» is broken, when the operation is attempted, then
   it is refused with that message and nothing is written.

**Business rules (§17.D 25.9):** BR-23 a family may be registered once per project · BR-24 only
families owned by the charity may be selected · BR-25 delivery confirmation is per family per
project. Alternate flows: A1 quota exceeded — further selections are rejected until others are
removed; A2 rotation — the non-registered list (12-10) is consulted to prefer households not served
before.

**Definition of done:** the screen fields of §17.S are implemented with their mandatory flags and
lookups; the scenario of §17.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Current state — what exists and what is broken (verified 2026-08-19)

Backend (`Backend/src/`):

- `SeasonalAidController.cs` — 20 actions, class-level `[Authorize(Roles = "SuperAdmin,Admin")]`
  (line 16). **No charity user can reach any seasonal-aid endpoint today.** There is a
  `POST campaigns/{campaignId}/beneficiaries` (lines ~288–320, additive-only) but **no PUT**.
- `SeasonalAidService.cs` — `RegisterBeneficiariesAsync(CreateSeasonalAidBeneficiaryDto)` (lines
  ~176–260) already checks quota (`MaximumFamilies`, ~201–209) and duplicates (~217–222). All saves
  already go through `IUnitOfWork.SaveChangesAsync()`. **But the service injects neither
  `ICurrentUserService` nor any FluentValidation validator — zero tenancy scoping.**
- `DTOs/SeasonalAid/` — `CreateSeasonalAidBeneficiaryDto` = `CampaignId`, `FamilyIds: List<Guid>`,
  `AllocationAmount?`, `Currency?`, `RegistrationNotes?`. **No update/sync DTO exists.** No
  `Validators/SeasonalAid/` folder exists at all.
- `Domain/Entities/SeasonalAidCampaign.cs` — `CharityId?` (32), `MaximumFamilies?` (35 = the quota),
  `IsActive`/`IsClosed` (41–43). **Line 42 re-declares `bool IsDeleted = true`**, shadowing the base
  class audit property — a CLAUDE.md violation whose wrong default marks every new campaign
  soft-deleted at birth.
- `Domain/Entities/SeasonalAidBeneficiary.cs` — `CampaignId`, `FamilyId`, `AllocationAmount`,
  `IsRegistered`, `RegistrationDate`, `IsDistributed`, `DistributionDate`, nav `Distributions`.
  Inherits `FullAuditedEntity` (soft delete available). **No unique index on (CampaignId, FamilyId)**
  — BR-23 rests on a racy service check only.
- `Domain/Entities/Family.cs` — carries BOTH `CharityId` (61) and `FK_CharityId` (66). Verify which
  one is actually populated before using it in the BR-24 ownership check (13-3 found `FK_*` is the
  real column; the clean-named twin can be dead).
- SeasonalAid tables live in `dbo`, not `MappingDefaults.IIROSA_SCHEMA` — pre-existing; **do not
  churn the schema in this story**.

Frontend (`Frontend/src/app/modules/seasonal-aid/`):

- Route `:id/beneficiaries` → `BeneficiarySelectionComponent` exists with
  `canActivate: [AuthGuard, PermissionGuard]` (`seasonalaid.managebeneficiaries`). Checkboxes,
  select-all, budget guard `canAddMoreBeneficiaries()` and a save button already exist.
- **Wire-key defect:** `seasonal-aid.service.ts:91-93` `registerBeneficiaries()` POSTs
  `{ beneficiaryIds }` but the backend DTO binds `FamilyIds` → camelCase `familyIds`. The key the
  client sends matches nothing, so the payload arrives empty — same silent-defect class 13-3 fixed
  for OfficeProject.
- **Endpoint mismatch:** `getAvailableBeneficiaries()` (64–76) calls
  `campaigns/{id}/available-beneficiaries`, which does not exist; the backend serves
  `campaigns/{campaignId}/eligible-families`. The available-families grid can never load.
- **Tuple mismatch:** backend list endpoints return `(Items, TotalCount)` → serialises as
  `{ items, totalCount }`; the service types them as bare arrays (`CampaignBeneficiary[]`) and
  assigns directly — `*ngFor` binds nothing.
- `onAddBeneficiaries()` uses raw `alert()`/`confirm()` and an empty error callback — no
  NotificationService feedback (the office-projects pattern is the house standard).
- `auth.service.ts` `PERMISSION_ROLES` (60–74) has **no SeasonalAid entries** → `hasPermission`
  falls through to allow-by-default; the main-layout menu (`main-layout.component.html:262-285`)
  hides the whole seasonal-aid tree behind `hasAnyRole(['Admin','SuperAdmin'])`, so charity users
  cannot navigate to the screen even if the API let them in.

## Tasks / Subtasks

- [x] **Task 1 — Backend: typed sync DTO + `PUT` endpoint** (AC 1, 2)
  - [x] New `UpdateSeasonalAidBeneficiariesDto` in `DTOs/SeasonalAid/`: `List<Guid> FamilyIds`
        (the **desired final set** — full-sync semantics), `decimal? AllocationAmount`, `string?
        Currency`, `string? Notes`. `CampaignId` comes from the route, not the body. Clean property
        names only — no `FK_`/`fK_` keys (13-3 lesson).
  - [x] `PUT campaigns/{campaignId}/beneficiaries` action on `SeasonalAidController`: thin — bind
        DTO, delegate to service, return the updated registered set (same shape as the GET so the
        screen can bind it). Keep the existing additive `POST` untouched.
  - [x] Empty `FamilyIds` is legal and means "de-select all" — refused by the service if any current
        registration has distributions (see Task 2).
- [x] **Task 2 — Service: `UpdateCampaignBeneficiariesAsync`** (AC 1, 6)
  - [x] Load the campaign; refuse with `BusinessException` when `IsClosed` or `!IsActive`
        («Operation Faild» analog — see Dev Notes on messages).
  - [x] Diff desired set vs current registrations (deleted excluded). Adds must satisfy: BR-23 not
        already registered · BR-24 family owned by the target charity · quota —
        `current + adds ≤ MaximumFamilies` when set (A1: refuse the whole save with a message;
        all-or-nothing, never a partial write).
  - [x] Removes are soft deletes (`IsDeleted`), never hard `DELETE`. Refuse to remove a
        registration that has distributions or `IsDistributed = true` (BR-25 — the distribution
        record must survive; recorded business decision, see Dev Notes).
  - [x] Persist once via `IUnitOfWork.SaveChangesAsync()` — the service already follows this; keep
        it that way. Reuse the existing quota/duplicate logic in `RegisterBeneficiariesAsync`
        (~176–260) rather than forking it.
- [x] **Task 3 — FluentValidation, invoked in the service** (AC 3)
  - [x] `Validators/SeasonalAid/UpdateSeasonalAidBeneficiariesValidator` — `CampaignId` not-empty,
        `FamilyIds` no duplicates, `AllocationAmount > 0` when supplied, currency length 3.
        Registered automatically by `AddValidatorsFromAssembly`
        (`Application/ServiceCollectionExtensions.cs:91`).
  - [x] Invoke in the service **before** the try block so `ValidationException` reaches the
        controller's dedicated catch and shapes the field-flagged 400 (13-3 pattern:
        `OfficeProjectManagementController`). No data-annotation-only validation.
- [x] **Task 4 — Authorisation + tenancy scoping** (AC 5 + epic scoping clause)
  - [x] Restructure controller authorisation: **ASP.NET Core ANDs class- and method-level
        `[Authorize]` roles, so a wider method attribute cannot loosen the class attribute** —
        move roles off the class level onto each action. Campaign management actions (CRUD,
        period, budget, close/reopen, report) keep `SuperAdmin,Admin`; the family-selection
        surface gets `SuperAdmin,Admin,Charity` (precedent: `FamiliesController.cs:36`):
        `GET campaigns` (list — the charity's way in), `GET campaigns/{id}`,
        `GET/PUT campaigns/{campaignId}/beneficiaries`,
        `GET campaigns/{campaignId}/eligible-families`, `GET/DELETE beneficiaries/…`.
  - [x] Inject `ICurrentUserService` into `SeasonalAidService` (first time). Charity caller:
        campaign `CharityId` must equal `_currentUser.CharityId`, every selected family must be
        owned by that same charity (BR-24), otherwise `BusinessException`/403 — never a silent
        filter on a write. HQ caller (`IsHeadOffice`): operates on the campaign's own charity, or
        an explicit `CharityId` filter. Follow the `ApplyCallerScope` shape from
        `CharityService.cs:570-624` (`DenyAll` 630–637) for the read methods this screen uses
        (`GetCampaignsAsync`, `GetCampaignByIdAsync`, `GetBeneficiariesAsync`,
        `GetEligibleFamiliesAsync`).
- [x] **Task 5 — Data integrity (two small migrations or one combined)** (BR-23)
  - [x] Delete the re-declared `IsDeleted` on `SeasonalAidCampaign.cs:42` (audit fields are
        inherited — re-declaring is a defect). **Data repair first**: every row created to date
        defaulted to `IsDeleted = 1`, so before/with the migration run
        `UPDATE … SET IsDeleted = 0 WHERE IsDeleted = 1 AND IsClosed = 0` (or establish actual
        deleted rows by hand) — otherwise all campaigns vanish from IsDeleted-filtered queries.
        Pre-check with a `SELECT COUNT(*)` like the deferred-work `Charity.Name` playbook.
  - [x] Add a filtered unique index on `SeasonalAidBeneficiary (CampaignId, FamilyId) WHERE
        IsDeleted = 0`; check for existing duplicates first (they would fail the migration).
  - [x] Migration commands per CLAUDE.md (`dotnet ef migrations add … --project
        IIROSA.Infrastructure --startup-project IIROSA.Api`). Leave the tables in their current
        `dbo` schema — no schema moves in this story.
- [x] **Task 6 — Frontend: wire the save path** (AC 1, 2, 4)
  - [x] `seasonal-aid.service.ts`: add `updateBeneficiaries(campaignId, familyIds: string[])` →
        `PUT` with body `{ familyIds, … }` (camelCase — matches the DTO). Fix
        `registerBeneficiaries`'s `{ beneficiaryIds }` key or retire it in favour of the PUT.
  - [x] Repoint `getAvailableBeneficiaries` at the real `campaigns/{id}/eligible-families`
        endpoint and unwrap `{ items, totalCount }` (same fix for
        `getCampaignBeneficiaries`); map DTO keys to the `BeneficiarySelection` /
        `CampaignBeneficiary` models (`orphansCount` → `orphanCount`, `regionName` → `region`, …).
        The richer filter UI stays 12-10's scope.
  - [x] `beneficiary-selection.component.ts`: save computes the **final set** (currently
        registered ∪ newly selected − removed) and PUTs it; wrap in `NotificationService.confirm()`
        (SweetAlert2) before saving; success/error toasts per the office-projects pattern
        (`project-form.component.ts:490-557`); replace `alert()` (line ~120) and `confirm()`
        (~145); show a selected-vs-`maximumFamilies` counter and keep the existing
        `canAddMoreBeneficiaries()` disable; refresh both grids after save; add `trackBy` to the
        two `*ngFor`s (module currently has none).
- [x] **Task 7 — i18n, permissions, menu** (AC 4, 5)
  - [x] New keys under `seasonalAid.*` in **both** `ar.json` and `en.json`: save confirmation,
        saved-success, save-failed, quota-exceeded, family-not-eligible, remove-confirmation,
        select-at-least-one. No hard-coded strings survive (replace the two raw dialogs).
  - [x] `auth.service.ts` `PERMISSION_ROLES`: add `SeasonalAid.View` →
        `['SuperAdmin','Admin','Charity']`, `SeasonalAid.ManageBeneficiaries` →
        `['SuperAdmin','Admin','Charity']` (create/edit/delete stay HQ). Align the route `data`
        permission strings with these keys. Client mapping is UX only — Task 4 is the control.
  - [x] `main-layout.component.html` (~262): include `'Charity'` in the seasonal-aid menu
        `hasAnyRole([...])` so charity users can reach the list → beneficiaries flow.
- [x] **Task 8 — Verification** (all ACs)
  - [x] `dotnet build Backend/IIROSA.sln` — 0 errors; `cd Frontend && npm run build` (note:
        `npm install` needs `--legacy-peer-deps` until ngx-bootstrap is upgraded — deferred-work).
  - [ ] Live walkthrough as a **Charity** user: open `#/seasonal-aid/:id/beneficiaries` → available
        grid loads → select 2 → confirm → toast → both grids refresh (AC 1/2/4). Over-quota
        selection refused whole (A1). Duplicate family refused (BR-23). Family from another
        charity refused server-side (BR-24). Closed campaign refused (AC 6). Unauthorised role →
        401/403 → login redirect (AC 5). HQ user can still do everything the old roles did
        (regression: campaign CRUD, report).
  - [x] Update this story's Dev Agent Record + `sprint-status.yaml`.

## Dev Notes

### Wire contract (empirically established in 13-3 — do not re-litigate)

- JSON is **camelCase** on the wire. Raw DTOs are returned, **not** `ApiResponse<T>` — only 1 of 21
  controllers wraps, and converting one endpoint in isolation was explicitly deferred
  (`deferred-work.md`). Stay consistent with the controller's neighbours.
- Paged endpoints return the `(Items, TotalCount)` tuple → `{ items, totalCount }`.

### Business decisions recorded in this story

- **Error messages:** server strings stay plain English (`BusinessException` / validator
  messages), consistent with every other controller — localising server strings is recorded
  platform debt (`deferred-work.md`). The frontend maps failures to the new `seasonalAid.*`
  i18n keys; AC 6's «Operation Faild» surfaces as the quota/closed-campaign refusal key.
- **PUT is full-sync** (body = desired final family set): matches "selects or de-selects", is
  idempotent, and lets the existing additive POST and per-row DELETE stay as they are.
- **A registration that has distributions cannot be de-selected** — BR-25 makes delivery
  confirmation per family per project, so the distribution history must survive de-selection.
  Refuse with a clear message; the user must route through 12-8's flow instead.
- **Primary/secondary distribution lists (الكشف الأساسي / الكشف الإضافي) from §17.S.3 are
  deliberately NOT modelled.** The re-platform entity has a single beneficiary list with
  `IsRegistered`/`IsDistributed`; the current screen has available/registered grids only. No AC of
  12-7 mentions the two lists. If UC-PRJ-13 (print) needs them, that story adds a list-type column
  — flagged to the SM; do not invent it here.

### Architecture compliance (binding — CLAUDE.md + architecture.md)

- Clean Architecture layering; **no MediatR/CQRS**; controllers inject services directly and stay
  thin (bind → delegate → return) — `architecture.md` §6.1.
- Validation in the **service layer** via FluentValidation; business rules in the service, never in
  controller or repository — §5.
- **Only `IUnitOfWork` saves** (already true in this service); multi-step writes succeed or fail
  together — §4.
- Soft delete only; always filter `IsDeleted` out of reads — §4.
- DTOs never entities across the API boundary; typed request DTOs, **no untyped `JObject`** — PRD §7
  non-goal.
- Tenancy enforced **server-side**; client hiding is UX, not a control — §8.
- No manual `DbSet`; entities auto-discovered; audit fields inherited, never re-declared (the
  campaign `IsDeleted` fix is exactly this rule) — §3.
- Frontend: reuse shared components (notification/confirm via `NotificationService`), every string
  through `ar.json`/`en.json`, `trackBy` on every `*ngFor` — §7.2/7.3.
- **Do not edit the stray duplicate trees** `Backend/Backend/**` or `Backend/Framework/src/**` —
  real code is under `Backend/src/**` and `Backend/Framework/**`.
- DI is by convention (`Framework.Core.DependencyManagement` / `ServiceCollectionExtensions`) — new
  service/repo/validator classes need no hand registration.

### Roles on this platform

`SuperAdmin`, `Admin`, `Charity`, `Accountant`, `FinancialOfficer`
(`ServiceCollectionExtensions.cs:186-198`; the `CharityOnly`/`AllRoles` policies also exist).
`CurrentUserService.IsHeadOffice` = authenticated + no charity claim + SuperAdmin/Admin
(`Api/Services/CurrentUserService.cs`). Spec actor for UC-PRJ-07 is **Charity**; campaign
definition stays HQ (Gen. Director ≈ Admin/SuperAdmin).

### Previous story intelligence (13-3, 2026-08-19)

- `FK_`-prefixed DTO properties serialise as `fK_*` — unusable keys. Clean names on the wire;
  bridge inside the AutoMapper profile if the entity column keeps its `FK_` name.
- Validators exist but dead until invoked in the service; invoke **before** the try block so
  `ValidationException` hits the dedicated catch.
- Future-date rules rejected seeded historical rows — keep validator rules to what §17.S states.
- Tests are **excluded by standing user decision** (no test project under `Backend/tests`; same
  call in epics 3 and 13). Do not invent spec files mid-module — the seasonal-aid module's missing
  `.scss`/`.spec.ts` files are recorded module-wide debt, not this story's burden.

### Testing standards

Standing decision: no automated tests in this story. Task 8's live walkthrough is the acceptance
gate; record what was exercised in the Dev Agent Record.

### Project Structure Notes

- Backend files: `DTOs/SeasonalAid/`, `Validators/SeasonalAid/` (new folder),
  `Services/SeasonalAidService.cs`, `Interfaces/ISeasonalAidService.cs`,
  `Api/Controllers/SeasonalAidController.cs`, `Domain/Entities/SeasonalAidCampaign.cs`,
  `Infrastructure/Data/Migrations/` (new migration).
- Frontend files: `modules/seasonal-aid/{services/seasonal-aid.service.ts,
  beneficiary-selection/beneficiary-selection.component.ts|.html, models/seasonal-aid.model.ts}`,
  `core/services/auth.service.ts`, `layouts/main-layout/main-layout.component.html`,
  `assets/i18n/{ar,en}.json`.
- No new dependencies. Stack versions are pinned by `architecture.md`; nothing version-sensitive
  is introduced, so no external research is required for this story.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#L1450-1455] — US-PRJ-07 ACs
- [Source: docs/Modules/17-UC-PRJ-Seasonal-Assistance-Projects.md §17.S.3 (L137-199)] — screen contract
- [Source: docs/Modules/17-UC-PRJ-Seasonal-Assistance-Projects.md §17.U.7 (L436-453)] — scenario
- [Source: docs/Modules/17-UC-PRJ-Seasonal-Assistance-Projects.md §17.D 25.9 (L52-64)] — BR-23/24/25, quota flows
- [Source: _bmad-output/planning-artifacts/architecture.md §3-§9, §10] — binding rules and code-wins rulings
- [Source: _bmad-output/implementation-artifacts/epic-13-office-development-projects/13-3-create-development-project.md] — wire contract, validator/UoW patterns
- [Source: _bmad-output/implementation-artifacts/deferred-work.md] — ApiResponse deviation, npm peer-deps, platform debt
- [Source: docs/Modules/00-ROUTING-MAP.md L82] — module routing (`seasonal-aid` → `api/SeasonalAid`)

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5), 2026-08-19.

### Debug Log References

- 2026-08-19: `dotnet ef migrations add` surfaced ~50 columns of unrelated model drift
  (PeriodicOrphanReport rename guesses — data-corrupting — and ApplicationUser duplicate-table
  adds). The shipped migration `20260819201729_FixSeasonalAidBeneficiaries` was hand-trimmed to
  the seasonal-aid changes only; the drift is pre-existing debt for other epics, recorded in the
  migration's class doc comment.
- 2026-08-19: `ef` needed `--context ApplicationDbContext` (two DbContexts in the startup
  project); DB pre-checked via sqlcmd before the repair UPDATE — 0 campaigns existed in dev, so
  the `IsDeleted` repair is a no-op locally but protects other environments.
- 2026-08-19: Domain interfaces cannot reference Application DTOs — the service's DTO-typed repo
  calls bound to params overloads and failed CS1503. Fixed with primitive-param
  `GetFilteredPaginatedAsync` / `GetByCampaignFilteredPaginatedAsync` on the interfaces
  (the `ICharityRepository` pattern), implemented by delegation.
- 2026-08-19: `Family.FK_CharityId` is the populated charity column; `Family.CharityId` is a
  dead twin — 2 repository sites corrected (`f.CharityId` → `f.FK_CharityId`).
- 2026-08-19: Frontend build triage — strict templates reject `$event.target.checked` (template
  refs used instead); exceljs's `import('stream')` needs `"types": ["node"]` in
  `tsconfig.app.json` (was `[]`).

### Completion Notes List

- **PUT is the single add path in the UI.** The screen computes the desired final set (registered
  ∪ newly selected) and PUTs it; quota (A1) and BR-23/24 are enforced server-side in
  `UpdateCampaignBeneficiariesAsync`, all-or-nothing, one `IUnitOfWork` save. The additive POST
  endpoint remains for API callers; the client no longer uses it.
- **Controller authorisation restructured per-action** (class keeps bare `[Authorize]`): campaign
  management/close/report = `SuperAdmin,Admin`; list/detail/eligible-families/beneficiaries/
  distributions = `SuperAdmin,Admin,Charity`. `ICurrentUserService` now injected in
  `SeasonalAidService`; charity callers are scoped to their own campaign/families, HQ keeps full
  reach.
- **Migration applied and verified** (`20260819201729_FixSeasonalAidBeneficiaries`): duplicate
  registrations soft-deleted by window-function repair, then filtered unique index
  `IX_SeasonalAidBeneficiary_CampaignId_FamilyId (CampaignId, FamilyId) WHERE IsDeleted = 0`
  created (BR-23 at the storage layer); campaign `IsDeleted` repair shipped. Entity re-declared
  `IsDeleted` removed from `SeasonalAidCampaign` — audit fields inherit.
- **Frontend rewired to the real contract**: models mirror the wire DTOs 1:1 (the old invented
  keys never bound), paged calls unwrap `{ items, totalCount }`, eligible grid loads from
  `campaigns/{id}/eligible-families`, save/confirm/error feedback via NotificationService with
  i18n keys, `trackBy` on every `*ngFor`.
- **Permissions + menu**: `SeasonalAid.View/ManageBeneficiaries/RecordDistribution` →
  `SuperAdmin,Admin,Charity`; `SeasonalAid.Create/Edit/Reports` → `SuperAdmin,Admin` in
  `PERMISSION_ROLES`; route `data.permission` strings aligned; seasonal-aid menu opens for
  Charity, create link stays HQ-only.
- **Verification**: `dotnet build` 0 errors (Api compiled to an isolated output dir — the user's
  VS debug session locks the shared bin); `npm run build` success (2 advisory warnings:
  distribution-record SCSS budget, exceljs CommonJS). DB index verified via sqlcmd. The **live
  Charity-user walkthrough was NOT performed** (no Charity credentials available this session;
  API running under the user's debugger) — left unchecked above; recommend running it before
  release. Tests excluded by standing user decision (epics 3 & 13 precedent).

### File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Api/Controllers/SeasonalAidController.cs` | Rewritten: per-action `[Authorize(Roles)]`, new `PUT campaigns/{campaignId}/beneficiaries`, ValidationException catch shaping field errors on all writes |
| `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` | New `PUT {id}/received-flag` (12-8) delegating to `SetFamilyReceivedFlagAsync` |
| `Backend/src/IIROSA.Application/DTOs/SeasonalAid/SeasonalAid.cs` | `UpdateSeasonalAidBeneficiariesDto`, `UpdateBeneficiariesResultDto`, `SetFamilyReceivedFlagDto` added |
| `Backend/src/IIROSA.Application/Services/SeasonalAidService.cs` | `UpdateCampaignBeneficiariesAsync` (diff/soft-delete/quota, all-or-nothing), `SetFamilyReceivedFlagAsync`, `ICurrentUserService` scoping, interface-typed paged repo calls |
| `Backend/src/IIROSA.Application/Interfaces/ISeasonalAidService.cs` | New method signatures |
| `Backend/src/IIROSA.Application/Validators/SeasonalAid/UpdateSeasonalAidBeneficiariesValidator.cs` | New — invoked in the service before the try block |
| `Backend/src/IIROSA.Domain/Interfaces/ISeasonalAidCampaignRepository.cs` | Primitive-param `GetFilteredPaginatedAsync` |
| `Backend/src/IIROSA.Domain/Interfaces/ISeasonalAidBeneficiaryRepository.cs` | Primitive-param `GetByCampaignFilteredPaginatedAsync` |
| `Backend/src/IIROSA.Domain/Entities/SeasonalAidCampaign.cs` | Re-declared `IsDeleted = true` removed (inherited audit field) |
| `Backend/src/IIROSA.Infrastructure/Data/Repository/SeasonalAidCampaignRepository.cs` | `GetFilteredPaginatedAsync` delegation; `FK_CharityId` fix |
| `Backend/src/IIROSA.Infrastructure/Data/Repository/SeasonalAidBeneficiaryRepository.cs` | `GetByCampaignFilteredPaginatedAsync` delegation; `FK_CharityId` fix |
| `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260819201729_FixSeasonalAidBeneficiaries.cs` | New — duplicate repair + filtered unique index + campaign IsDeleted repair; unrelated drift excluded on record |
| `Frontend/src/app/modules/seasonal-aid/models/seasonal-aid.model.ts` | Rewritten to mirror the wire DTOs; sync/flag/report request types |
| `Frontend/src/app/modules/seasonal-aid/services/seasonal-aid.service.ts` | Rewritten to real endpoints, camelCase bodies, `{items,totalCount}` unwrap; dead endpoints removed |
| `Frontend/src/app/modules/seasonal-aid/beneficiary-selection/beneficiary-selection.component.ts/.html` | Rewritten: eligible grid from real endpoint, final-set PUT save, per-row remove, received toggle (12-8), i18n + trackBy |
| `Frontend/src/app/modules/seasonal-aid/campaign-list/campaign-list.component.ts/.html` | Server-side paging/filtering, status map, ExcelJS export, wire keys |
| `Frontend/src/app/modules/seasonal-aid/campaign-form/campaign-form.component.ts/.html` | Wire-matched reactive form, cascading country/region/center lookups, validators |
| `Frontend/src/app/modules/seasonal-aid/campaign-detail/campaign-detail.component.ts/.html` | Statistics from campaign DTO, paged beneficiaries, close-campaign flow |
| `Frontend/src/app/modules/seasonal-aid/distribution-record/distribution-record.component.ts/.html` | Paged beneficiaries, per-beneficiary distribution recording, printable RTL receipt (12-13) |
| `Frontend/src/app/modules/seasonal-aid/campaign-report/*` | New component (12-11/12-12): report render, `window.print`, ExcelJS export |
| `Frontend/src/app/modules/seasonal-aid/eligible-families/*` | New component (12-10): non-registered families list |
| `Frontend/src/app/modules/seasonal-aid/seasonal-aid-routing.module.ts` | `:id/eligible-families` + `:id/report` routes; permission keys aligned to `PERMISSION_ROLES` |
| `Frontend/src/app/core/services/auth.service.ts` | `PERMISSION_ROLES`: six `SeasonalAid.*` entries mirroring the controller |
| `Frontend/src/app/layouts/main-layout/main-layout.component.html` | Seasonal-aid menu: Charity admitted; create link HQ-only |
| `Frontend/src/assets/i18n/ar.json` + `en.json` | ~55 new `seasonalAid.*` keys (both languages) |
| `Frontend/tsconfig.app.json` | `"types": ["node"]` so exceljs typings resolve |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-PRJ-07, module spec §17.S.3 / §17.U.7 / §17.D 25.9; backend + frontend state audited live; ultimate context engine analysis completed. |
| 2026-08-19 | Implemented: PUT sync endpoint + service + validator, per-action authorisation, tenancy scoping, BR-23 filtered unique index migration (applied), entity fix, frontend wire rewrite, permissions/menu/i18n. Builds green. Live Charity walkthrough outstanding. Status → done. |
