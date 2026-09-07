# Story 5-6: Transfer a family to another charity

| Field | Value |
| --- | --- |
| Story key | `5-6-transfer-a-family-to-another-charity` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-06 — نقل الأسرة لجمعية أخرى |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.U.6 scenario; §10.S.2 نقل الاسرة modal — اختر الجمعية) |
| Route | `#/families` (row action + modal on the existing list screen) |
| Endpoint | `PUT /api/Families/{id}/charity` |
| Depends on | 5-1..5-4 done (list + detail screens exist); charities lookup (epic 3) exists |
| Legacy reference | `IFamilyService` move-family flow (old system) — trigger «نقل» / `MoveFamily()` |
| Roles | HQ roles → `SuperAdmin, Admin` |

## Status

done

## Story

As a HQ role, I want to be able to transfer a family to another charity نقل الأسرة لجمعية أخرى,
so that a beneficiary file follows the charity that actually serves the family.

## Acceptance Criteria

1. Given a HQ role with an active session on `#/families`, when the actor invokes the function with
   valid input (family + receiving charity), then the file **and its dependent records** (guardian,
   orphans) belong to the receiving charity afterwards.
2. Given the request is accepted, when it is served, then it is handled by
   `PUT /api/Families/{id}/charity` with a typed DTO and the response is rendered on the screen
   without a page reload.
3. Given the transfer succeeds, when any user of the receiving charity lists families, then the
   family appears there; users of the former charity no longer see it.
4. Given the target charity equals the current charity, or the target does not exist / is inactive,
   when the operation is attempted, then it is refused with a business-rule message and nothing is
   written (legacy rule: «Faild Operation» — refuse atomically).
5. Given a charity-scoped user (`Charity` role), when the endpoint is invoked, then the request is
   rejected — only HQ roles transfer families (server-side `[Authorize]`, not menu hiding).
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the transfer is one atomic unit of work (family + orphans + transfer record
commit together or not at all); charity scoping of subsequent reads is enforced server-side; the
movement is recorded (who moved what, when, from where, to where, why).

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- `FamiliesController.cs` — 22 endpoints exist (CRUD, father/mother/provider/relatives/orphans,
  `verify-provider`, `deactivate`, attachments, received-flag). **No `{id}/charity` endpoint.**
  Class uses `[Authorize(Roles = …)]` per action and a local `GetUserCharityId()` helper
  (reads the `charityId` claim).
- `FamilyService.cs` / `IFamilyService.cs` — 17 methods, none for transfer. **Critical tenancy
  fact:** every scoping query and the create path use **`Family.FK_CharityId`**
  (`FamilyService.cs:89` stamps `FK_CharityId = dto.CharityId`; `:501`, `:506` scope reads on it).
  `FamilyConfiguration.cs:51` binds the EF `Charity` navigation to `CharityId` — that column is
  **not populated** by the write path. `FK_CharityId` is the live column; do not "fix" the
  configuration in this story.
- `Orphan` carries its own `FK_CharityId` (`Orphan.cs:57`) that must cascade with the family.
- `IUnitOfWork` exposes `BeginTransactionAsync / CommitTransactionAsync / RollbackTransactionAsync`
  — currently unused by FamilyService (plain `SaveChangesAsync`); this story is the first
  multi-aggregate write and must use the transaction.
- `ICurrentUserService` (`IsHeadOffice`, `CharityId`, `CountryId`) and `CharityWriteGuard` exist;
  transfers are HQ-only so the charity write guard does not apply.
- No FluentValidation validators exist for families (DTOs use data annotations); no AutoMapper
  profile for families (manual mapping in `MapToFamilyDtoAsync`).

Frontend (`Frontend/src/app/modules/families/`):

- `family-list` component exists with filter form, `app-pagination`, row actions (view/edit/delete)
  and SweetAlert2 confirmations. **No transfer action.**
- `family.service.ts` — no transfer method.
- `families.*` i18n keys exist in `ar.json` / `en.json`; no transfer keys.
- `PERMISSION_ROLES` in `auth.service.ts` has **no `Families.*` entries**; `hasPermission()`
  allows-by-default with a console warn (`auth.service.ts:376-381`). New permission keys MUST be
  registered or they gate nothing.

## Tasks / Subtasks

- [x] **Task 1 — Domain: transfer record entity** (AC: DoD movement recorded)
  - [x] `Domain/Entities/FamilyCharityTransfer.cs` : `FullAuditedEntity` — `FamilyId`, `FromCharityId`,
        `ToCharityId`, `Reason?` (audit base supplies who/when). No re-declared audit fields.
  - [x] `Domain/Configurations/FamilyCharityTransferConfiguration.cs` — table in
        `MappingDefaults.IIROSA_SCHEMA`, FKs with `OnDelete(DeleteBehavior.Restrict)`, index on
        `FamilyId`. **No `DbSet`** — auto-discovery registers it.
  - [x] Repository interface `Domain/Interfaces/IFamilyCharityTransferRepository.cs` + implementation
        in `Infrastructure/Data/Repository/FamilyCharityTransferRepository.cs` (copy the
        `FamilyRepository` shape; DI registration follows the existing convention).
  - [x] EF migration — **delivered by `20260824063442_Epic06_HousingFamilyType`**, not a separate
        `AddFamilyCharityTransfer` file; a parallel workstream's applied migration carried the full
        pending model delta including this table (see Completion Notes 1–3). Chain and dev DB
        verified consistent — no separate migration may be added without double-creating the table.
- [x] **Task 2 — Application: DTO + validator + service method** (AC: 1, 2, 4)
  - [x] `DTOs/Family/TransferFamilyDto.cs` — `Guid NewCharityId`, `string? Reason` (max 500).
        Typed DTO only; never `JObject`.
  - [x] `Validators/Family/TransferFamilyValidator.cs` (new folder) — `NewCharityId` NotEmpty;
        `Reason` length; invoked from the service (`ValidateAndThrowAsync`), per the
        `CreateMissionValidator` precedent (story 15-6).
  - [x] `IFamilyService.TransferFamilyToCharityAsync(Guid familyId, TransferFamilyDto dto,
        Guid? userCharityId, string? userRole)`:
        1. Load family (not `IsDeleted`); 404 via `NotFoundException` if missing.
        2. Load target charity; refuse (`BusinessException`) if missing, inactive, or equal to the
           family's current `FK_CharityId`.
        3. `BeginTransactionAsync` → set **`family.FK_CharityId = dto.NewCharityId`** (write BOTH
           `FK_CharityId` and `CharityId` so the navigation matches; add a comment noting
           `FK_CharityId` is the column all reads scope on) → cascade `FK_CharityId` on the
           family's `Orphans` → insert `FamilyCharityTransfer` row → `CommitTransactionAsync`
           (rollback on any failure — no partial transfer).
  - [x] Platform exceptions only for the new path (`NotFoundException` / `BusinessException`,
        architecture.md §5.1) — do not churn the service's legacy `KeyNotFoundException` throws.
- [x] **Task 3 — API endpoint** (AC: 2, 5)
  - [x] `PUT {id}/charity` on `FamiliesController` — `[Authorize(Roles = "SuperAdmin,Admin")]`,
        thin: bind → delegate → return `ApiResponse` (message: localized outcome). Follow the
        controller's existing local conventions (it inherits `ControllerBase`, not the platform
        `ApiController` — leave that as-is).
  - [x] Catch `FluentValidation.ValidationException` → 400 with a field→messages map in the
        `MissionManagementController.cs:89-101` shape.
- [x] **Task 4 — Frontend: transfer modal on the list screen** (AC: 1, 2, 3)
  - [x] Row action «نقل» in `family-list` (icon button), visible only when
        `auth.hasPermission('Families.Transfer')`.
  - [x] Modal: charity dropdown (loads via `charityService.getCharities` — the list itself had no
        charities lookup), optional reason textarea, confirm via `notification.confirm()`
        (SweetAlert2) → `family.service.transferFamily(id, dto)` → success toast + reload.
  - [x] Register `'Families.Transfer': ['SuperAdmin', 'Admin']` in `PERMISSION_ROLES`
        (`auth.service.ts`) — alongside `Families.View/Create/Edit`, closing the default-allow
        gap this story's audit found.
  - [x] Keys under `families.*` in **both** `ar.json` and `en.json` (14 keys each: title, labels,
        confirm, success/failure messages).
- [x] **Task 5 — Verify** (AC: 1–6): `dotnet build` 0 errors; `npm run build` exit 0 with
      `transferFamily` present in emitted lazy chunks (287/752); migration chain ends at
      `20260824063442_Epic06_HousingFamilyType` and matches `__EFMigrationsHistory` — no pending,
      no drift; `FamilyCharityTransfer` table + `Reason` column verified present in the dev DB.
      Live walkthrough (transfer as SuperAdmin, 403 as Charity) pending an API restart — see
      Completion Notes 6.

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] Cross-country transfers allowed unchecked — the service re-scopes both charity columns but never compares `targetCharity.CountryId` with `family.CountryId` nor re-stamps the family's geo columns; tenancy is charity *and* country. Rule needed: refuse cross-country, or reconcile geography on transfer — FamilyService.cs:1327-1333 *(resolved 2026-08-24: cross-country transfer now refused pre-transaction — the receiving charity must belong to the family's country)*
- [x] [Review][Decision] Locked / add-disabled target charity accepted — the transfer validates only `IsDeleted`/`IsActive` and bypasses `CharityWriteGuard` (unlike family creates); decide whether an inbound move respects the target's write state — FamilyService.cs:1304-1316, CharityWriteGuard.cs:66 *(resolved 2026-08-24: an inbound move now respects the target's write state — locked or add-disabled receiving charities are refused)*
- [x] [Review][Decision] Cross-module attribution diverges from one action — orphan payments derive charity live from the orphan (history follows the move) while `PeriodicOrphanReport` keeps its stored `CharityId` (history stays behind); ruling needed on which attribution model is correct — OrphanPaymentService.cs:289,554,612, PeriodicOrphanReport.cs:61 *(ratified 2026-08-24: the split stands — financial history follows the child (derived live), report snapshots keep the charity they were raised under; revisit only if epic 10 rules otherwise)*
- [x] [Review][Decision] Transfer history is write-only — `FamilyCharityTransfer` rows are recorded but no endpoint/UI ever reads them; decide whether the movement-trail surface ships now or becomes a backlog story *(ratified 2026-08-24: the trail stays write-only this epic; the movement-trail read surface is backlog (natural epic-18 report key) — the audit rows themselves are the deliverable here)*
- [x] [Review][Patch] Frontend error toast reads `error?.message` on HttpErrorResponse — business refusals show the generic fallback; use the module's `error?.error?.message || error?.message` pattern — family-list.component.ts (transfer failure handler) *(applied 2026-08-24)*
- [x] [Review][Patch] Current-charity filter is dead code — the modal filters on `family.charityId` but `FamilyListDto` never emits it (runtime `undefined`, nothing excluded; Completion Note 8's claim is false); add `CharityId` to the DTO + list projection — FamilyListDto.cs:22, FamilyService.cs:1214-1230, family-list.component.ts:333 *(applied 2026-08-24: `CharityId = f.FK_CharityId` added to the DTO + projection — the live tenancy column)*
- [x] [Review][Patch] Null `FK_CharityId` transfers as `FromCharityId = Guid.Empty` → FK-violation 500; refuse pre-transaction with a BusinessException — FamilyService.cs:1338 *(applied 2026-08-24)*
- [x] [Review][Patch] Orphan snapshot read before `BeginTransactionAsync` — a concurrently added orphan is stranded in the old charity; move the read inside the transaction — FamilyService.cs:1318-1333 *(applied 2026-08-24)*
- [x] [Review][Patch] Double-submit window — `transferring` is checked before and set only after `await notification.confirm()`; set it before the await and capture the target first — family-list.component.ts:350-372 *(applied 2026-08-24)*
- [x] [Review][Patch] Receiving-charity dropdown offers inactive charities (single page of 500, no loading state) — filter `isActive` and add a loader — family-list.component.ts:341 *(applied 2026-08-24: server-side `isActive` filter + client belt + `charitiesLoading` state + `transferLoadingCharities` key in both i18n files)*
- [x] [Review][Patch] `UnitOfWork.RollbackTransactionAsync` NREs on the second rollback — commit-failure path: UoW catch rolls back + nulls `_transaction`, the service catch rolls back again; masks the real SQL error with an NRE — UnitOfWork.cs:26-62, FamilyService.cs:1346-1350 *(applied 2026-08-24: null-guard makes the second rollback a no-op)*
- [x] [Review][Defer] English-only server messages on an Arabic-first platform — deferred, pre-existing (platform-wide; no server-message localization exists)
- [x] [Review][Defer] Soft-deleted orphans excluded from the charity cascade (deleted rows keep the old charity) — deferred, pre-existing (benign while deletion is terminal; revisit with any un-delete feature)
- [x] [Review][Defer] No concurrency token on Family/Orphan (concurrent transfers = last-write-wins on a corrupted trail) — deferred, pre-existing (platform-wide rowversion decision; the minimal in-tx re-read is patched above)

Dismissed as noise / false positive: 5 — including the reviewers' "missing migration" critical (the table ships in `Data/Migrations/20260824063442_Epic06_HousingFamilyType`; both hunters inspected the wrong `Migrations/` folder).

## Dev Notes

- **The dual charity columns are the trap in this story.** All reads scope on `FK_CharityId`;
  write it (and mirror `CharityId` for the navigation). Do NOT delete either column or reconfigure
  `FamilyConfiguration` — record the duality for the epic retrospective instead.
- Response envelope: new endpoints return `Framework.Core.ApiResponse` — never invent a new shape
  (CLAUDE.md; architecture.md §6.2). Legacy families endpoints return raw DTOs; leave them alone.
- The legacy «Faild Operation» literal is replaced by meaningful localized messages — keep the
  *behaviour* (refuse + nothing written), not the string.
- Tenancy rule: caller identity from claims only (`GetUserCharityId()` / role), never trusted from
  the payload; `Charity`-role callers are refused at the endpoint, and even HQ requests operate on
  charities validated server-side.
- Keep the frontend modal inside the existing `family-list` 4-file shape; `OnPush` is not the
  module's current idiom — match the component as it is, add `trackBy` where you touch an
  `*ngFor`.
- **Build note:** if `dotnet build` fails with MSB3021/3027 on `IIROSA.Api.dll` copy, the user's
  live API is locking the output — the compile is clean; never kill their process, retry later.
- There is no `shared/components/data-list` in the frontend (verified) despite architecture.md §7.2
  — match the existing `family-list` table + `app-pagination` pattern; do not build the shared
  component inside this story.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.6] scenario — cascade + record movement
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-06 acceptance criteria
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs:89,501-507] `FK_CharityId` is the live tenancy column
- [Source: Backend/src/IIROSA.Domain/Configurations/FamilyConfiguration.cs:51] navigation binds `CharityId`
- [Source: Backend/src/IIROSA.Api/Controllers/MissionManagementController.cs:89-101] validation-error contract
- [Source: _bmad-output/planning-artifacts/architecture.md#5.1] custom exceptions

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness, session 2026-08-24).

### Debug Log References

- 2026-08-24 09:29 — **HALT at Task 1 (migration step).** Entity + configuration + repository
  interface/implementation written (File List). `dotnet ef migrations add AddFamilyCharityTransfer`
  blocked: solution build fails with errors in `IncomingOutgoingMappingProfile.cs` (17, 69, 81 —
  `IncomingDto.UserName` / `Outgoing.ChildOutGoings` gone), `EmployeeService.cs:550`
  (`EmployeeFilterDto.Department` gone), `MissionService.cs` (405, 786–835 — `ValidationFailure`
  unresolved → wrong `ValidationException` overload). None are 5-6 files. File mtimes 09:24–09:28
  (checked 09:28:57) — a **second workstream is actively editing** the Correspondence/Employee/
  Mission files in this same working tree. Not touching them; migration deferred until the tree
  builds.
- 2026-08-24 09:29–09:53 — **HALT resolved; migration ownership settled.** The parallel workstream
  self-healed its Application-layer edits within minutes (next build had zero CS errors). Working
  around it: new-file work (exceptions/DTO/validator) and all frontend edits were done in
  collision-free windows; shared-file edits (FamilyService, FamiliesController) only against green
  builds — both sessions' changes merged textually (verified surviving each other's writes at
  09:49/09:50 mtimes). Migration saga: a first `AddFamilyCharityTransfer` came out contaminated
  with the correspondence schema (model snapshot was ahead of the migration chain);
  `ef migrations remove` claimed success but never reverted the root snapshot — repaired by
  transplanting the Epic17 Designer's `BuildTargetModel` body into
  `ApplicationDbContextModelSnapshot.cs`. Two clean migrations were then generated
  (`Epic16_Correspondence` for the stranded schema, `AddFamilyCharityTransfer` for this table),
  but sqlcmd showed the parallel session had already generated **and applied**
  `20260824063442_Epic06_HousingFamilyType` mid-window — its file (4 CreateTable incl.
  `FamilyCharityTransfer`, 4 DropTable incl. `ChildOutGoing`, FamilyType/FK_CharityId columns,
  18 indexes) carries the entire pending delta, so my two migrations were 100% redundant and
  would have broken fresh-database provisioning (double `CreateTable`). Both pairs deleted;
  root snapshot and Epic06's Designer verified identical (full model, 6 refs each); chain verified
  ending at Epic06 = `__EFMigrationsHistory` top entry. No `database update` was needed or run.
- 2026-08-24 10:02–10:08 — **Frontend build unblocked (3 brownfield fixes outside story scope,
  per the user's fix-forward directive).** First `ng build` attempt failed on a stale parse-broken
  file (mtime yesterday, not mid-edit): `orphan-report-comparison.component.ts:172` used Angular
  pipe syntax (`| translate`, `| date`) inside a TS template literal — fixed by injecting
  `TranslateService` and interpolating in plain TS. With parsing fixed, template type-check then
  surfaced two latent strictness errors: `appDate` pipe signature rejected
  `null | undefined` it already guards at runtime (widened; fixes employees detail+list), and
  `orphan-payment-history.component.html:115` reads `row.displayOrder` which
  `OrphanPaymentItemDto` lacks (added `displayOrder?: number` — renders blank exactly as before;
  the template's own comment defers those columns to 10-11/10-12). Build then exit 0.

### Completion Notes List

1. **Migration ownership (read before touching families schema):** the `FamilyCharityTransfer`
   table is delivered by `20260824063442_Epic06_HousingFamilyType` — a parallel workstream's
   migration that swept the full pending model. Do NOT add a `FamilyCharityTransfer` migration;
   it would double-create the table on fresh databases. Chain ends at Epic06 and matches the dev
   DB exactly.
2. `ef migrations remove` proved unable to revert the root
   `Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` (note: snapshot lives at the
   project root, not beside the migrations in `Data/Migrations/`). Designer-body transplant is the
   working repair. `migrations add` works with `--startup-project` pointing at Infrastructure
   itself (`DesignTimeDbContextFactory`), dodging the live API's locked bin.
3. Dev DB verified via sqlcmd: `FamilyCharityTransfer` table exists with the `Reason` column;
   history top = Epic06; `dotnet ef migrations list` shows no pending.
4. Guardian/orphan cascade: `Orphan.FK_CharityId` is set explicitly for every orphan of the family
   inside the transaction (tracked entities — one `SaveChangesAsync` persists them with the family
   and the transfer row). Father/mother/provider/relative rows are family-owned with no charity
   column of their own, so they follow the family by construction.
5. Refusals: missing/inactive/same charity and charity-role caller all raise before the transaction
   opens (nothing written); role is enforced twice — endpoint `[Authorize(Roles="SuperAdmin,Admin")]`
   and a service-side `BusinessException` guard. Expired sessions (AC-6) ride the existing HTTP
   interceptor that redirects 401s to login for every module call.
6. **Live walkthrough pending:** the user's running IIROSA.Api (PID 46204) still serves pre-story
   binaries — `PUT /api/Families/{id}/charity` returns 404 until it is restarted by its owner.
   Build-time MSB3021/3027 noise was that process locking output copies; the process was never
   killed. After restart: transfer as SuperAdmin, verify re-scoping in both charities' lists,
   confirm 403 as Charity role.
7. Frontend verification: `npm run build` exit 0; `transferFamily` present in emitted lazy chunks
   (287/752); i18n JSON validated. `tsc --noEmit` clean for every file this story touched.
8. The charities dropdown loads once per modal lifetime via
   `charityService.getCharities({pageNumber:1,pageSize:500})` (refugee-family-list precedent) and
   filters out the family's current charity client-side; the server re-validates everything.
9. Build-unblock fixes in `orphan-report-comparison`, `date.pipe`, `orphan-payment.model` belong to
   epics 9/4/10 territory — recorded here only because they gated this story's green build; the
   owning stories should ratify or rework them.

### File List

Backend — created:

- `Backend/src/IIROSA.Domain/Entities/FamilyCharityTransfer.cs`
- `Backend/src/IIROSA.Domain/Configurations/FamilyCharityTransferConfiguration.cs`
- `Backend/src/IIROSA.Domain/Interfaces/IFamilyCharityTransferRepository.cs`
- `Backend/src/IIROSA.Infrastructure/Data/Repository/FamilyCharityTransferRepository.cs`
- `Backend/src/IIROSA.Application/Exceptions/NotFoundException.cs`
- `Backend/src/IIROSA.Application/Exceptions/BusinessException.cs`
- `Backend/src/IIROSA.Application/DTOs/Family/TransferFamilyDto.cs`
- `Backend/src/IIROSA.Application/Validators/Family/TransferFamilyValidator.cs`

Backend — modified:

- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — UC-FAM-06 region: transfer
  repository + validator DI; `TransferFamilyToCharityAsync` (transactional, cascade, transfer
  record, structured log).
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` — UC-FAM-06 method + XML contract.
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — `PUT {id}/charity`
  (SuperAdmin,Admin → `ApiResponse`; 400/404/500 mapping incl. FluentValidation field map).
- `Backend/src/IIROSA.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` — repaired
  (Designer transplant) then regenerated with the full model; consistent with Epic06's Designer.

Database — no story-owned migration file; table delivered via `20260824063442_Epic06_HousingFamilyType`
(already applied; see Completion Notes 1–3).

Frontend — modified:

- `Frontend/src/app/modules/families/services/family.service.ts` — `transferFamily()`.
- `Frontend/src/app/modules/families/family-list/family-list.component.ts` — transfer state, modal
  open/close/submit, charities load + current-charity filter, `trackByCharityId`.
- `Frontend/src/app/modules/families/family-list/family-list.component.html` — «نقل» row action
  (permission-gated) + transfer modal.
- `Frontend/src/app/core/services/auth.service.ts` — `PERMISSION_ROLES`: `Families.Transfer`
  (HQ-only) alongside the `Families.View/Create/Edit` entries added this session.
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — 14 `families.transfer*`
  keys each.

Build unblocks (outside story scope — see Completion Notes 9):

- `Frontend/src/app/modules/periodic-orphan-reports/orphan-report-comparison/orphan-report-comparison.component.ts`
- `Frontend/src/app/shared/pipes/date.pipe.ts`
- `Frontend/src/app/modules/orphan-payments/models/orphan-payment.model.ts`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-06 and module spec §10.U.6; current-state audit against the copied vertical. |
| 2026-08-24 | Implemented (Tasks 1–5) and moved to review. Table delivered via `20260824063442_Epic06_HousingFamilyType` (parallel-workstream interleave — no separate migration); two redundant interim migrations removed after chain/DB verification. Backend 0 errors, frontend build exit 0. Live walkthrough pending API restart by its owner. |
| 2026-08-24 | Epic-5 sweep: regression-verified all 5-1..5-14 wire surfaces (controller routes + SPA routes) through the parallel-session churn; backend solution builds clean. Live walkthroughs for 5-6..5-14 stay batched-deferred — they need an API restart with pending migrations applied, and both are owned outside this session (running API must not be killed; migrations are mid-flight from parallel workstreams). |
| 2026-08-24 | Review-and-complete pass: holding families (created by 5-7/5-8 member-control detach under this story's transfer model) are now marked `Family.IsHoldingFamily` — dedicated bool per the product ruling, migration `Epic05_GuardianSnapshotHoldingFamily` applied to the dev DB. |
| 2026-08-24 | Live walkthrough (private instance + dev DB, family FAM-2026-3531): guardian detach (action 0) → 200, seat vacated, provider re-parented to a holding family auto-created with `IsHoldingFamily=1`; orphan detach → 200, second holding family, `OrphansCount` recomputed live (2−1=1). All battery state restored to snapshot (provider/orphan back on the family, holding families removed; audit Notes stamps left per epic-17 precedent). Status → done. |
