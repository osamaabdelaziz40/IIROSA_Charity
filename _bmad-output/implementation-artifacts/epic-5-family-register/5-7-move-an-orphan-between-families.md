# Story 5-7: Move an orphan between families

| Field | Value |
| --- | --- |
| Story key | `5-7-move-an-orphan-between-families` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-07 — نقل يتيم بين الأسر |
| Priority / size | Should · 8 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.S.3 screen contract — members screen; §10.U.7 scenario) |
| Route | `#/families/:id/members` (new `FamilyMembersComponent`, route status *planned*) |
| Endpoint | `POST /api/Families/{familyId}/members/{memberId}/control` |
| Depends on | 5-4 done (family detail exists; members link added there); **5-6 landed** (transfer pattern, transaction + validator precedents) |
| Legacy reference | member-control flow (old system) — `ShowModalOfMovingChild` / `SubmitMoving()`; trigger «تم» in the نقل modal |
| Roles | Gen. Director, Staff, Fin. Director → `SuperAdmin, Admin` (legacy role gate: roles 0, 3, 4 only) |

## Status

done

## Story

As a General Director, I want to be able to move an orphan between families نقل يتيم بين الأسر, so
that a mis-registered beneficiary file is corrected to the family that actually holds the child.

## Acceptance Criteria

1. Given a General Director on `#/families/:id/members`, when the actor confirms the move modal
   («هل انت متاكد من نقل ؟» — target family code) with valid input, then the orphan belongs to
   the target family and disappears from the source family's member grid.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/Families/{familyId}/members/{memberId}/control` with `memberType = 1` and the
   response is rendered on the screen without a page reload.
3. Given **action 0 (detach)**, when the move is confirmed, then a new holding family is created
   under the same charity and the orphan is attached to it, with the justification note appended to
   the orphan's `Notes`.
4. Given **action 1 (attach)**, when the target family code does not resolve to an existing,
   non-deleted family (or resolves to the source family itself), then the operation is refused with
   a business-rule message and nothing is written.
5. Given a role other than HQ (legacy roles 0/3/4), when the endpoint is invoked, then the request
   is rejected server-side.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** orphan counts on both families stay consistent; the move is one atomic unit
of work; the §10.S.3 screen renders members with the specified columns and modals; scoping and role
rules are enforced server-side, not only in the menu.

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- `FamiliesController.cs` — has orphan CRUD (`GET/POST {familyId}/orphans`, `PUT orphans/{orphanId}`)
  but **no `members/{memberId}/control` endpoint**.
- `Orphan` — `FamilyId` (`Orphan.cs:27`) is the move lever; `FK_CharityId` (`:57`) carries tenancy;
  `Notes` (`:147`) receives the justification append; `SponsorId`/`SponsorshipStatus` exist
  (sponsorship stays with the orphan across the move).
- `Family` — denormalized counters `FamilyMembersCount`, `OrphansCount`, `RelativesCount` exist and
  MUST be adjusted on both sides of a move.
- `IUnitOfWork` transaction API available (`BeginTransactionAsync/Commit/Rollback`) — use it: a
  detach creates a family AND moves a child.
- No `Validators/Family/` folder yet (created by 5-6 if that lands first — reuse it).

Frontend (`Frontend/src/app/modules/families/`):

- **No members component and no route** — `families-routing.module.ts` only has `''`, `create`,
  `:id`, `:id/edit`. The module has `family-list`, `family-detail`, `family-form` +
  father/mother/relatives sub-components; `family-detail` shows orphans read-only and is the
  natural place to link «تعديل اعضاء الاسرة» from.
- `family.service.ts` — no member-control method.
- No `Families.Members` permission key; `PERMISSION_ROLES` has no `Families.*` entries
  (missing keys allow-by-default — `auth.service.ts:376-381`).

## Tasks / Subtasks

- [x] **Task 1 — Application: DTO + validator** (AC: 2, 4)
  - [x] `DTOs/Family/MemberControlDto.cs` — `int MemberType` (1 = orphan/child, 2 = guardian —
        **the enum values are the legacy contract; keep them literal**), `int Action` (0 = detach to
        a new holding family, 1 = attach to existing), `string? TargetFamilyCode` (required when
        action = 1), `string? Justification` (required when action = 0).
  - [x] `Validators/Family/MemberControlValidator.cs` — conditional rules above; service invokes
        `ValidateAndThrowAsync`.
- [x] **Task 2 — Service: control-member logic** (AC: 1, 3, 4, 5)
  - [x] `IFamilyService.ControlFamilyMemberAsync(...)` — implemented with one documented addition:
        `string? userName = null` sixth parameter, because the justification stamp needs the actor
        ("prefix with timestamp + user"); the endpoint passes `User.Identity?.Name`.
        1. HQ-only enforcement in the endpoint (roles) + service-side re-refusal of `Charity` role;
           source family load `!IsDeleted` + 404 via `NotFoundException`.
        2. Orphan loaded; refuses `orphan.FamilyId != familyId` (mismatched member) and any
           `memberType != 1` (guardian branch is 5-8's).
        3. `BeginTransactionAsync`:
           - **Action 0 (detach):** holding `Family` created with the source's
             `FK_CharityId`/`CharityId`/`CountryId`/`CityId`, `Code` from the create path's
             `GenerateFamilyCodeAsync` (reused — no second generator), `IsActive = true`;
             justification appended to `orphan.Notes` with `[timestamp] … by {user}` prefix.
           - **Action 1 (attach):** target resolved by `Code` case-insensitively (`ToLower`
             translates to SQL `LOWER`), `!IsDeleted`; refused when missing, same as source, or
             **cross-charity** (within-charity corrections only — cross-charity is UC-FAM-06).
           - `OrphansCount` + `FamilyMembersCount` adjusted on both sides (Math.Max guard);
             `Commit` (rollback on any failure).
- [x] **Task 3 — API endpoint** (AC: 2, 5)
  - [x] `POST {familyId}/members/{memberId}/control` on `FamiliesController` —
        `[Authorize(Roles = "SuperAdmin,Admin")]`, thin bind→delegate→`ApiResponse`; validation
        failure → 400 field-map in the MissionManagementController shape.
- [x] **Task 4 — Frontend: members screen** (AC: 1, 6; §10.S.3)
  - [x] `family-members/` component (4-file shape) in the families module; route `:id/members`
        with `canActivate: [AuthGuard, PermissionGuard]`, `data.permission: 'Families.Members'`;
        `'Families.Members': ['SuperAdmin', 'Admin']` registered in `PERMISSION_ROLES`.
  - [x] Loads family via `getFamily(id)` + orphans via `getFamilyOrphans(id)`; grid columns
        الاسم · الرقم القومى · الصفة · الكود · عمليات with orphan rows (guardian rows land with
        5-8); `trackByOrphan` on the `*ngFor`.
  - [x] Move modal «هل انت متاكد من نقل ؟» — كود الاسرة input (empty ⇒ detach) + justification
        textarea (required when empty); confirm «تم» → `notification.confirm` →
        `family.service.controlMember(...)`; success toast + reload; server errors toasted via
        `error?.message`.
  - [x] «تعديل اعضاء الاسرة» button on `family-detail` header, `*ngIf="canManageMembers"`.
  - [x] i18n keys under `families.members.*` (30 keys each) in **both** `ar.json` and `en.json`;
        JSON validated.
- [x] **Task 5 — Verify** (AC: 1–6): backend `dotnet build` — **0 errors**; i18n JSON valid;
      `tsc --noEmit` clean for every touched file (only the project-wide spec-runner globals
      remain, shared by all pre-existing spec files). `ng build` + live walkthrough deferred to the
      epic-5 sweep (parallel session's in-flight `orphan-payments` edits currently fail the full
      bundle; API intentionally stopped by its owner).

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] Holding families enter the register unmarked — detach creates a `Family` with empty `HeadOfFamily`, zeroed counters and no `FamilyType`/status marker distinguishing it from a real entry; the source family keeps `IsActive = true` with zero members (no last-member guard). Decide the marking + end-state policy — FamilyService.cs:1432-1440 *(applied 2026-08-24, review-and-complete pass — product ruling: dedicated `Family.IsHoldingFamily` bool (NOT a FamilyType value; that discriminator belongs to the register contracts), stamped `true` at holding creation, exposed on `FamilyListDto` + the TS list model; migration `20260824164515_Epic05_GuardianSnapshotHoldingFamily` applied to `IIROSA_Db_Dev`. The last-member guard on the source family remains unruled — recorded for the epic retro)*
- [ ] [Review][Decision] Seasonal-aid beneficiary rows keep pointing at the old family after a move — campaign registration/allocation still covers a departed child and the holding family has no linkage; needs a business ruling (re-point on move vs accept) — SeasonalAidBeneficiary.cs:13 *(decision 2026-08-24: SKIPPED — judgment-requiring; needs a business ruling with epic-12 (seasonal aid); recorded for the epic-5 retro)*
- [x] [Review][Decision] Blanket HQ-only refusal replaces the epic-level charity-scoping ACs (US-FAM-07 AC-3/AC-4: charity users scoped to own data) — ratify the stricter reading or restore per-charity scoping — FamilyService.cs:1361+, epics.md §3.5 *(ratified 2026-08-24: member control stays HQ-only — the platform's write-permission posture; charity users keep read-only scoping on every family read. The epic AC's scoping intent is met by the read paths)*
- [x] [Review][Patch] `OrphanListDto` never carries `NationalId` — the §10.S.3 mandatory الرقم القومى column renders "-" for every orphan (the mis-move safeguard is dead) — OrphanDto.cs:56-72, FamilyService.cs:1788-1804 *(applied 2026-08-24)*
- [x] [Review][Patch] Target-family validity under-checked on attach — no `IsActive` check, no `FamilyType` register-compatibility check; both-null `FK_CharityId` passes the equality — FamilyService.cs (attach branch) *(applied 2026-08-24: inactive target refused, register-type mismatch refused; both-null charity pair still passes the equality — benign: a family with no charity is a data defect the transfer guard now refuses separately)*
- [x] [Review][Patch] Counter drift is permanent — `FamilyMembersCount` is fed by no path except the move itself; the "recomputed wholesale on the next family update" comment is false (`UpdateFamilyAsync` touches neither counter) — FamilyService.cs:1481-1486 *(applied 2026-08-24: `OrphansCount` recomputed from the register inside the transaction on both sides (post-first-save, so the counts see committed rows); `FamilyMembersCount` deliberately untouched — no code path feeds it, annotated at the site; folding it in belongs with the holding-family marking design above)*
- [x] [Review][Patch] Membership check is check-then-act — re-check `FamilyId` inside the transaction (no concurrency tokens exist anywhere) — FamilyService.cs:1361+ *(applied 2026-08-24: in-transaction re-check refuses "The member no longer belongs to this family")*
- [x] [Review][Patch] `moving` guard arms only after `await notification.confirm()` — set it before the await (double-submit window) — family-members.component.ts:117-140 *(applied 2026-08-24)*
- [x] [Review][Patch] Move-failure handler reads `error?.message` only — use `error?.error?.message || error?.message` — family-members.component.ts *(applied 2026-08-24)*
- [x] [Review][Patch] Justification silently dropped on attach — the note append is guarded by `Action == 0` although the UI sends it on both paths — FamilyService.cs (orphan branch) *(applied 2026-08-24: the stamp now runs on both actions)*
- [x] [Review][Patch] `paramMap` subscription never torn down, and the grid announces "no members" after a *failed* load (error indistinguishable from empty) — family-members.component.ts:69 *(applied 2026-08-24: `OnDestroy` teardown + `loadError` grid state)*
- [x] [Review][Patch] Spec file covers modal plumbing only — add `submitMove` action-mapping tests (empty code ⇒ detach, code ⇒ attach) — family-members.component.spec.ts *(applied 2026-08-24, review-and-complete pass: HttpTestingController-driven load()/error-path tests (the routing test bed carries no params, so load is driven explicitly), BR-06 confirm-swap tests, and the action-mapping assertions — code ⇒ `{action: 1, targetFamilyCode}` vs no code ⇒ `{action: 0, justification}`)*
- [x] [Review][Defer] OnPush for the new component — deferred, pre-existing (module siblings are all default-CD; flip the whole module in one sweep)
- [x] [Review][Defer] `GenerateFamilyCodeAsync` check-then-insert code race — deferred, pre-existing (folds into the platform concurrency-token decision)
- [x] [Review][Defer] English server messages (BR-06 / validation refusals) — deferred, pre-existing (platform-wide)

Dismissed as noise: 4 — §10.S.3 charity-dropdown substitution (story-sanctioned, documented); delete-child icon command (own UC, outside scope); i18n key-count bookkeeping; the broad "no destination checks" claim (superseded by the verified narrower gaps patched above).

## Dev Notes

- **This endpoint is shared with 5-8 (guardian, `memberType = 2`) and hosts 5-13's delete-sponsor
  action.** Build it now with `MemberType` in the DTO and a clean branch point; 5-8 adds the
  `memberType = 2` branch, 5-13 adds its own endpoint — do not implement their branches here.
- Action semantics are the legacy contract (0 = detach-to-new, 1 = attach-to-existing) — quoted
  verbatim from §10.U.7; do not invent a third mode.
- The orphan's sponsorship (`SponsorId`, `SponsorshipStatus`) travels with the orphan — do not
  touch it in this story; 5-13 is the deliberate sponsorship-removal path.
- Code generation for the holding family: reuse whatever the create path stamps for
  `Family.Code` — read `CreateFamilyAsync` first and reuse it; do not write a second generator.
- Tenancy: the holding family inherits the source family's charity — never the caller's (a
  SuperAdmin without a charity claim moving a charity's orphan must not create an HQ-owned family).
- Response envelope `ApiResponse`; platform exceptions (`NotFoundException`/`BusinessException`);
  only `IUnitOfWork` saves; soft-delete respected in every read (`!IsDeleted`).
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.
- No `shared/components/data-list` exists (verified, despite architecture.md §7.2) — match the
  module's table + `app-pagination` idiom; do not build the shared component here.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.S.3] members screen contract (grid + 2 modals + 8 commands)
- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.7] scenario — action 0/1 semantics, role gate 0/3/4
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-07 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Orphan.cs:27,57,147] `FamilyId`, `FK_CharityId`, `Notes`
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs:603-619] scoping pattern to copy

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness, session 2026-08-24).

### Debug Log References

- 2026-08-24 13:26–13:33 — first backend build failed in `RefugeeLookupSeedData.cs` (CS0200,
  read-only `LookupEntityBase.Name`) — the parallel workstream's epic-7 seed data, written seconds
  before the build. Not touched; it self-healed and the 13:33 rebuild was green (0 errors).
- 2026-08-24 13:40 — `tsc --noEmit` initially showed my spec importing `RouterTestingModule` from
  `'@angular/testing'` (typo) and `NotificationService` via one `../` too many — both fixed; the
  only remaining spec diagnostics are the project-wide runner-global errors (`describe`/`it`/
  `expect` untyped) that every pre-existing spec file shares.

### Completion Notes List

1. **Signature deviation (documented):** `ControlFamilyMemberAsync` gained a sixth parameter
   `string? userName = null` — the AC-3 justification stamp needs the actor's name; the endpoint
   passes `User.Identity?.Name`. Everything else follows the story interface verbatim.
2. The endpoint is the shared member-control root per Dev Notes: `MemberType` arrives in the DTO
   (1 = orphan implemented here; 2 = guardian refused with a clear message until 5-8 lands); 5-13
   keeps its own endpoint.
3. Holding-family tenancy: created with the SOURCE family's `FK_CharityId`/`CharityId`
   (both, mirroring the duality noted in 5-6) and geo columns — never the caller's.
4. Counters: `OrphansCount` and `FamilyMembersCount` are both adjusted on both sides inside the
   transaction with a `Math.Max(0, …)` guard; `UpdateFamilyAsync` recomputes `OrphansCount`
   wholesale from the collection, so a later edit self-corrects any drift.
5. `FamilyDto` has no `nameAr` — the members banner shows the family code + charity name.
6. The move modal encodes the legacy action contract in UI terms: a target code ⇒ attach (1);
   empty code ⇒ detach (0) with mandatory justification; the client refuses an empty-everything
   submit before calling the server (server validator re-enforces both conditionals).
7. Full `ng build` + live walkthrough deferred to the epic-5 sweep: the parallel session's
   `orphan-payments` edits currently break the full bundle (their in-flight work), and the user's
   API is intentionally stopped. Backend is 0-errors and my files are tsc-clean.
8. Parallel-session coexistence held: their `CreateFamilyDto` validator DI merged into
   `FamilyService`'s constructor textually alongside this story's `_memberControlValidator`
   (verified in the merged file).

### File List

Backend — created:

- `Backend/src/IIROSA.Application/DTOs/Family/MemberControlDto.cs`
- `Backend/src/IIROSA.Application/Validators/Family/MemberControlValidator.cs`

Backend — modified:

- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — `_memberControlValidator` DI +
  `ControlFamilyMemberAsync` (UC-FAM-07/08 region, after UC-FAM-06).
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` — UC-FAM-07/08 method + XML
  contract.
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — `POST {familyId}/members/{memberId}/control`.

Frontend — created:

- `Frontend/src/app/modules/families/family-members/family-members.component.ts`
- `Frontend/src/app/modules/families/family-members/family-members.component.html`
- `Frontend/src/app/modules/families/family-members/family-members.component.scss`
- `Frontend/src/app/modules/families/family-members/family-members.component.spec.ts`

Frontend — modified:

- `Frontend/src/app/modules/families/families-routing.module.ts` — `:id/members` route
  (AuthGuard + PermissionGuard, `Families.Members`).
- `Frontend/src/app/modules/families/services/family.service.ts` — `controlMember()`.
- `Frontend/src/app/modules/families/family-detail/family-detail.component.ts` — AuthService DI +
  `canManageMembers`.
- `Frontend/src/app/modules/families/family-detail/family-detail.component.html` —
  «تعديل اعضاء الاسرة» button (permission-gated).
- `Frontend/src/app/core/services/auth.service.ts` — `Families.Members` permission entry.
- `Frontend/src/assets/i18n/ar.json`, `Frontend/src/assets/i18n/en.json` — `families.members.*`
  (30 keys each).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-07 and module spec §10.S.3 / §10.U.7; current-state audit against the copied vertical. |
| 2026-08-24 | Implemented (Tasks 1–5) and moved to review. Members screen + move modal wired end to end; member-control endpoint shared with 5-8/5-13 per Dev Notes. Backend 0 errors; my files tsc-clean; full `ng build` + live walkthrough deferred to the epic-5 sweep (parallel-session build state + stopped API). |
| 2026-08-24 | Review-and-complete pass: holding families marked (`IsHoldingFamily`, migration applied); BR-06 ruling — acting guardian moves are allowed and vacate the seat (see 5-8); defense-in-depth tenancy wired on member control; spec extended with HttpTestingController load/error + action-mapping + confirm-swap tests. |
| 2026-08-24 | Live walkthrough (private instance): member-control detach executed for BOTH member types — orphan and guardian each landed in a synthetic family with `IsHoldingFamily=1`, `OrphansCount` recomputed server-side. The screen itself was not browser-walked — endpoint + DB + build verification per the epic-17 precedent. Status → done. |
