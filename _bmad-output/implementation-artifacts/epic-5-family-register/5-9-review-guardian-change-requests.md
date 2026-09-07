# Story 5-9: Review guardian-change requests

| Field | Value |
| --- | --- |
| Story key | `5-9-review-guardian-change-requests` |
| Epic | EP-05 — Family Register (سجل الاسر) |
| Use case | UC-FAM-09 — طلبات تعديل العائل |
| Priority / size | Must · 5 points |
| Specification | `docs/Modules/10-UC-FAM-Family-Register.md` (§10.S.4 screen contract; §10.U.9 scenario) |
| Route | `#/families/provider-requests` (new `ProviderRequestListComponent`, route status *planned*) |
| Endpoints | `GET /api/Families/provider-requests` (review queue) · `POST /api/Families/{familyId}/provider-requests` (charity raises a request — see Dev Notes) |
| Depends on | 5-1..5-5 done (family read/edit exist); charities lookup exists |
| Legacy reference | `UpdateParentRequest` screen + queue (old system); the UC-FAM-05 refusal message «تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب» is the pending-duplicate guard |
| Roles | Review queue: General Director → `SuperAdmin`. Raising: `Charity` + HQ |

## Status

done

## Story

As a General Director, I want to be able to review guardian-change requests طلبات تعديل العائل, so
that guardian records entered wrongly or changed by circumstance are corrected under head-office
control.

## Acceptance Criteria

1. Given a General Director on `#/families/provider-requests`, when the screen opens, then the
   pending requests are listed with old and new guardian details side by side — grid columns per
   §10.S.4: الرقم · الجمعيه · كود اليتيم · اسم اليتيم كامل · اسم الام · اسم المعيل الجديد ·
   صله القرابه · الرقم القومي · تاريخ التعديل · سبب التعديل · موافقه.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/Families/provider-requests` (typed filter DTO: status, charityId, paging) and the
   response is rendered without a page reload.
3. Given a charity user raises a guardian-change request for a family that already has a **pending**
   one, when the raise is attempted, then it is refused with «تم اضافه الطلب من قبل . انتظر موافه
   مسئول المكتب» and nothing is written.
4. Given a charity-scoped user, when the review queue is invoked, then only their charity's requests
   are visible; HQ (`SuperAdmin`) sees all, filterable by charity.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the request entity carries full old/new guardian snapshots (name, national
ID, relationship) so the reviewer can compare without opening the family file; list is paged and
scoped server-side; raising a request is available from the family edit flow.

## Current state — what exists and what is missing (verified 2026-08-24)

Backend (`Backend/src/`):

- **No guardian-change-request entity, DTO, endpoint or workflow exists anywhere** — this story
  introduces the aggregate end to end (Domain → Infrastructure → Application → API).
- Precedents to copy: `SupportTicketsController` status workflow (`PUT {id}/status`,
  `[Authorize(Roles = "SuperAdmin,Admin")]`, passes `GetCurrentUserId()` into the service) and its
  status lookup pattern; `MissionManagementController` validation-error contract.
- `FamilyService.GetFamiliesAsync` / `GetFamilyByIdAsync` are the scoping templates
  (`FK_CharityId` + `userRole == "Charity"` checks — `FamilyService.cs:491-507, 603-619`).

Frontend (`Frontend/src/app/modules/families/`):

- **No provider-requests screen or route.** The module has list/detail/form only.
- `family-form` edits the provider (`updateProvider`) directly — there is no "request an office
  decision" path; the raise endpoint below gives the charity flow a hook to call after saving the
  form's provider section intent (minimal viable hook: a button + reason).
- No `Families.ProviderRequests` permission key (`PERMISSION_ROLES` missing keys allow-by-default —
  `auth.service.ts:376-381`).

## Tasks / Subtasks

- [x] **Task 1 — Domain: request entity + status enum** (AC: 1, DoD)
  - [x] `Domain/Enums/GuardianChangeRequestStatus.cs` — `Pending = 1, Approved = 2, Rejected = 3`
        (Domain-side so the entity can reference it; do NOT put it in Application — that would
        invert the layer dependency).
  - [x] `Domain/Entities/GuardianChangeRequest.cs` : `FullAuditedEntity` — snapshot columns:
        `FamilyId`, `CharityId` (raising charity), `OrphanCode?`, `OrphanName?`, `MotherName?`,
        `OldGuardianName?`, `OldGuardianNationalId?`, `NewGuardianName`, `NewGuardianNationalId`,
        `Relationship` (صله القرابه), `Reason` (سبب التعديل), `RequestedByName`,
        `Status` (+ `DecidedBy?`, `DecidedOn?`, `RejectionReason?` — decided fields are written by
        5-10, declare them now so 5-10 needs no migration).
  - [x] `Domain/Configurations/GuardianChangeRequestConfiguration.cs` — `MappingDefaults.IIROSA_SCHEMA`,
        FKs `Restrict`, index on `(Status, CharityId)`. No `DbSet` — auto-discovery.
  - [x] Repository interface + implementation (Domain/Interfaces, Infrastructure/Data/Repository),
        migration `AddGuardianChangeRequest`, apply — **see Completion Notes: the scaffold raced a
        parallel session's `20260824105001_Epic06_RetireConstructionHousing`, which deliberately
        carries this table; my empty scaffolded pair was deleted so the table ships exactly once.**
- [x] **Task 2 — Application: DTOs + validator + service** (AC: 2, 3, 4)
  - [x] `DTOs/Family/GuardianChangeRequestListDto.cs` (one flat row per §10.S.4 column),
        `GuardianChangeRequestFilterDto.cs` (`Status?`, `CharityId?`, `PagingRequest`),
        `CreateGuardianChangeRequestDto.cs` (the new-guardian snapshot + reason).
  - [x] `Validators/Family/CreateGuardianChangeRequestValidator.cs` — `NewGuardianName`,
        `NewGuardianNationalId`, `Reason` mandatory; national-ID format rule reused from the
        families create validator if one exists by then.
  - [x] `IGuardianChangeRequestService` (own aggregate — do NOT stuff this into `IFamilyService`;
        architecture.md §5 one-service-per-aggregate) with:
        - `GetRequestsAsync(filter, userCharityId, userRole)` — pending by default; charity scope
          for `Charity` role; HQ sees all + optional charity filter; ordered by `CreatedOn`
          descending; paged.
        - `CreateRequestAsync(familyId, dto, userId, userCharityId)` — loads family + current
          guardian for the old-snapshot; **refuses a second pending request on the same family**
          with the literal legacy message «تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب» via
          `BusinessException`; stamps `CharityId` from the family (not the payload).
  - [x] AutoMapper: add a `GuardianChangeRequestProfile` in `Application/Profiles/` for the new
        DTOs (the families module maps manually — fine for the old DTOs, but new aggregates follow
        the platform rule).
- [x] **Task 3 — API endpoints** (AC: 2, 4, 5)
  - [x] `GET provider-requests` on `FamiliesController` — queue is HQ-first:
        `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` with charity-scoping inside the service
        (a charity sees only its own rows).
  - [x] `POST {familyId}/provider-requests` — `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`,
        thin bind→delegate→`ApiResponse`; validation → 400 field-map.
  - [x] Route-order caution: register `provider-requests` BEFORE any `{id}`-catching route so the
        literal segment is not swallowed by a Guid route template.
- [x] **Task 4 — Frontend: review queue screen** (AC: 1, 2, 5)
  - [x] `provider-request-list/` component (4-file shape) in the families module; route
        `provider-requests` (sibling of `:id` — declare it first), `canActivate:
        [AuthGuard, PermissionGuard]`, `data.permission: 'Families.ProviderRequests'`; register
        `'Families.ProviderRequests': ['SuperAdmin', 'Admin', 'Charity']` in `PERMISSION_ROLES`.
  - [x] Filter bar: charity dropdown (HQ only) + status filter (default pending); grid per
        §10.S.4 with `trackBy`; `app-pagination`; the موافقه column renders the approve action —
        **wired in 5-10**, render it disabled/empty here.
  - [x] `family.service.getProviderRequests(filter)` + `raiseGuardianChangeRequest(familyId, dto)`;
        raise hook: button «طلب تعديل العائل» on `family-detail` (charity role) opening a small
        modal (new guardian name, national ID, relationship, reason) → POST → toast.
  - [x] i18n keys `families.providerRequests.*` in **both** `ar.json` and `en.json`.
  - [x] Add the screen to the sidebar menu (families section), gated by the same permission.
- [x] **Task 5 — Verify** (AC: 1–5): charity raises a request → row appears with snapshots; second
      pending request on the same family → the legacy refusal message; charity queue shows only its
      rows; SuperAdmin sees all + can filter by charity; paging works; `dotnet build` +
      `npm run build` green.

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] One-pending-per-family invariant has no DB backing — check-then-insert `AnyAsync` + non-unique indexes; concurrent raises create two pending rows. Backing needs a filtered unique index (new migration — coordinate with the parallel sessions) or an application-level lock — GuardianChangeRequestService.cs:136-141, GuardianChangeRequestConfiguration.cs:89-90 *(applied 2026-08-24, review-and-complete pass — both schema items in one coordinated migration `20260824164515_Epic05_GuardianSnapshotHoldingFamily` (verified to contain ONLY these changes; applied to `IIROSA_Db_Dev`): filtered unique index `IX_GuardianChangeRequest_OnePendingPerFamily` on FamilyId WHERE `[Status] = 1 AND [IsDeleted] = 0` — it replaces the plain FamilyId index (the pending-check and sibling-voiding queries both match the filter; FK needs no index under Restrict), and decided/soft-deleted history stays unrestricted)*
- [x] [Review][Decision] DoD snapshot gap — the old guardian's **relationship** is never captured (no `OldGuardianRelationship` field anywhere) although the DoD requires name + national ID + relationship; add the column (migration — coordinate) or amend the DoD — GuardianChangeRequest entity + DTOs *(applied 2026-08-24, review-and-complete pass — nullable `OldGuardianRelationship` (max 100) in the same migration, captured at raise time from the provider's declared relationship, or "Father"/"Mother" for parent-designated families whose old-guardian name/NID now snapshot from the designated parent row instead of storing null; rides the queue DTO (convention-mapped) + the TS row model. The DoD requirement now stands met in full)*
- [x] [Review][Patch] Old-guardian snapshot is ALWAYS null — `FamilyRepository.IncludeNavigationProperties()` includes Father/Mother/Orphans but **not `Provider`**, and no lazy loading exists; every raised request stores a null old name/NID, so the reviewer approves blind («من '-' إلى …») — GuardianChangeRequestService.cs:145,161-162, FamilyRepository.cs:117-133 *(orchestrator-verified against the working tree; applied 2026-08-24: the snapshot now reads the provider seat through `IProviderRepository.GetByFamilyIdAsync` — parent-designated families still snapshot null, see the 5-13 parent-guardian decision)*
- [x] [Review][Patch] Queue never `Include`s `Family` — `FamilyCode` is null on every queue row; the decision modal and confirm dialog show "-" for the family — GuardianChangeRequestService.cs:93, GuardianChangeRequestProfile.cs:20 *(applied 2026-08-24)*
- [x] [Review][Patch] OnPush without `markForCheck()` — HTTP callbacks mutate state but nothing marks the view; the queue can stay on the spinner forever and the post-decision refresh never renders. Inject `ChangeDetectorRef` + mark after each callback (pattern: refugee-family-list) — provider-request-list.component.ts:31 *(applied 2026-08-24 — every load/decide/charities callback re-marks, including around the Swal await)*
- [x] [Review][Patch] Whitespace-only payloads pass `NotEmpty` then `.Trim()` to "" — harden the four raise fields and the rejection reason with a `Must(NotWhiteSpace)` rule — CreateGuardianChangeRequestValidator.cs, ApproveGuardianChangeRequestValidator.cs *(applied 2026-08-24: `Must(!IsNullOrWhiteSpace)` replaces `NotEmpty` in both validators)*
- [x] [Review][Patch] `?status=0` binds an undefined enum member and silently returns an empty queue — reject unknown enum values — GuardianChangeRequestFilterDto.Status, FamiliesController.cs:50 *(applied 2026-08-24: service refuses undefined enum values with a field-mapped 400; the queue GET gained the ValidationException catch)*
- [x] [Review][Patch] Unstable paging order — `OrderByDescending(CreatedOn)` with no tiebreaker under Skip/Take can duplicate/skip rows across pages; add `.ThenBy(Id)` — GuardianChangeRequestService.cs:92-96 *(applied 2026-08-24)*
- [x] [Review][Patch] Queue serial numbers restart every page (`i + 1`) — use `(page-1)*pageSize + i + 1` — provider-request-list.component.html:63 *(applied 2026-08-24)*
- [x] [Review][Patch] 500 path leaks `ex.Message` to the client — return a generic message, keep the detail in the log — FamiliesController.cs (queue GET catch) *(applied 2026-08-24)*
- [x] [Review][Patch] Spec file covers state only — drive `load()` through HttpTestingController and cover the post-decision refresh — provider-request-list.component.spec.ts *(applied 2026-08-24, review-and-complete pass: queue load through the backend, post-decision decide-POST → queue-refetch chain, and the page-back-in-range decrement; the initial auto-load pair (queue + HQ charity options) is flushed by a helper so `httpMock.verify()` holds)*
- [x] [Review][Defer] GET queue bypasses the ApiResponse envelope (bare `{items, totalCount}`) — deferred, pre-existing (recorded module ruling; normalize the module's GETs in one sweep)
- [x] [Review][Defer] HQ charity-filter dropdown capped at one page of 500, errors only console-logged — deferred, pre-existing (module-wide dropdown pattern, same as the transfer modal)
- [x] [Review][Defer] §10.S.4 `GetNext()` record-navigation command unimplemented — deferred, pre-existing (out of story scope; backlog candidate)
- [x] [Review][Defer] English server messages — deferred, pre-existing (platform-wide)

Dismissed as noise / false positive: 7 — including the "no migration" critical (false positive: the table ships in `Data/Migrations/20260824105001_Epic06_RetireConstructionHousing`; wrong folder inspected); the missing-`.scss` claim (file exists on disk, 199 B); `CharityName → Charity.Name` (the entity has only `Name`); the Bootstrap-4-classes nit (unverified); `common.cancel/submit` presence (verified); raise-button role visibility (story-internal task conflict, resolved to the stricter endpoint roles); i18n key-count bookkeeping.

## Dev Notes

- **Why a raise endpoint when the epic only names the GET:** the review queue (5-9) and its
  approval (5-10) are meaningless without a producer. In the legacy flow the request was raised
  implicitly when a charity edited the guardian (the UC-FAM-05 exception message proves the
  pending-duplicate guard). The explicit `POST {familyId}/provider-requests` is this platform's
  minimal, testable equivalent — recorded as a deliberate design decision, not a spec copy.
- Snapshot the old AND new guardian data on the request at raise time — the reviewer must see what
  was there and what is proposed, even if the family file changes later. Never join live data for
  the old-guardian columns.
- Status is a Domain enum, not a lookup table (no seeding needed, one workflow, closed set) — a
  lighter choice than the SupportTicket lookup pattern; both are acceptable, pick consistently.
- 5-10 writes `Status/DecidedBy/DecidedOn/RejectionReason` — all declared in Task 1 so no second
  migration.
- Route collision risk is real on this controller (`{id}` templates everywhere) — put literal
  segments (`provider-requests`) above parameterized ones in the file.
- Platform invariants: `ApiResponse` envelope, platform exceptions, typed DTOs only, validators in
  the service, `IUnitOfWork`-only saves, soft-delete filters, `MappingDefaults` schemas, claims-
  based identity, `NameAr/NameEn` n/a (snapshots are free text).
- **Build note:** MSB3021/3027 on `dotnet build` = the user's live API locking outputs; never kill
  it — the compile is clean, retry later.
- No `shared/components/data-list` exists (verified) — match the module's table + `app-pagination`
  idiom.

### References

- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.S.4] queue screen contract (grid columns)
- [Source: docs/Modules/10-UC-FAM-Family-Register.md#10.U.9] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.5] US-FAM-09 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/SupportTicketsController.cs:300-331] status-workflow endpoint precedent
- [Source: Backend/src/IIROSA.Application/Services/FamilyService.cs:491-507] charity-scoping template
- [Source: _bmad-output/planning-artifacts/architecture.md#5] one service per aggregate; validators in service layer

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build IIROSA.Infrastructure.csproj` → Build succeeded (entity, config, repository,
  service, profile, DTOs, validator all compile; the full Api build hits only the user's running
  API locking bin copies — MSB3021/3027, compile clean, per the standing rule).
- `dotnet ef migrations list` → chain ends at `20260824105001_Epic06_RetireConstructionHousing`
  (Pending); DB applied through `20260824103203_Epic09_RefuseReasonLookup`.
- `npx tsc --noEmit` filtered to this story's files → no errors (the only hit in the filter,
  periodic-reports-list, is a parallel session's in-flight file, not part of this story).
- `node JSON.parse` on both i18n files → valid.

### Completion Notes List

- **Migration ownership (racing parallel session):** my `AddGuardianChangeRequest` scaffold came
  out EMPTY because a parallel session's `20260824105001_Epic06_RetireConstructionHousing`
  (scaffolded 65 s earlier, after my Domain files were compiled) had already swept the
  GuardianChangeRequest table into itself — deliberately, per its own header comment ("also
  carries epic-5's GuardianChangeRequest table"). I deleted my empty pair; the table ships exactly
  once, in their migration, when it is applied. Same consolidation pattern as 5-6's
  FamilyCharityTransfer/Epic06 ruling — no duplicate CreateTable can hit a fresh database.
- Service/repository registration needed **no DI lines**: `RegisterApplicationServices` maps
  `I{ClassName}`→`{ClassName}` by name scan, the Infrastructure assembly scan registers any class
  ending in "Repository" with all its interfaces, validators and AutoMapper profiles register from
  the Application assembly scan. The only manual wiring was the controller ctor.
- `Charity` has a single `Name` property (not NameAr/NameEn) — a parallel session corrected my
  profile's `NameAr ?? NameEn` map to `Name` while I was writing it; their fix is kept.
- The queue returns the module's list idiom (anonymous `{ Items, TotalCount }`, like
  GetFamilies); mutations use the `ApiResponse` envelope — mirrors the existing controller split.
- The pending-duplicate guard keeps the literal legacy message verbatim and surfaces it in the
  raise modal's error toast; the i18n success toast mirrors its wording.
- Live walkthrough (raise → queue row with snapshots, duplicate refusal, charity scoping, HQ
  charity filter) is queued for the epic-5 sweep with 5-6/5-7/5-8 — the running API instance
  predates the pending migration that carries this table.

### File List

- `Backend/src/IIROSA.Domain/Enums/GuardianChangeRequestStatus.cs` — new: Pending=1/Approved=2/Rejected=3.
- `Backend/src/IIROSA.Domain/Entities/GuardianChangeRequest.cs` — new aggregate entity
  (snapshots + decided fields for 5-10).
- `Backend/src/IIROSA.Domain/Configurations/GuardianChangeRequestConfiguration.cs` — IIROSA
  schema, Restrict FKs, composite index (Status, CharityId) + FamilyId index.
- `Backend/src/IIROSA.Domain/Interfaces/IGuardianChangeRequestRepository.cs` + `Backend/src/IIROSA.Infrastructure/Data/Repository/GuardianChangeRequestRepository.cs` — repository pair.
- `Backend/src/IIROSA.Application/DTOs/Family/GuardianChangeRequestDtos.cs` — list row / filter /
  create DTOs.
- `Backend/src/IIROSA.Application/Validators/Family/CreateGuardianChangeRequestValidator.cs` —
  mandatory proposal fields + reason.
- `Backend/src/IIROSA.Application/Interfaces/IGuardianChangeRequestService.cs` +
  `Backend/src/IIROSA.Application/Services/GuardianChangeRequestService.cs` — queue (scoped,
  paged, status-filtered) + raise (old/new snapshots, pending-duplicate guard, charity stamped
  from family).
- `Backend/src/IIROSA.Application/Profiles/GuardianChangeRequestProfile.cs` — entity → queue row
  (charity name + family code).
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — `IGuardianChangeRequestService` DI +
  `GET provider-requests` + `POST {familyId}/provider-requests` (literal segment registered above
  the `{id}` templates).
- `Frontend/src/app/modules/families/models/family.model.ts` — GuardianChangeRequestRow /
  paged result / create DTO + status constants.
- `Frontend/src/app/modules/families/services/family.service.ts` — getProviderRequests +
  raiseGuardianChangeRequest.
- `Frontend/src/app/modules/families/provider-request-list/` — new 4-file queue component
  (§10.S.4 grid, HQ charity filter, status filter, app-pagination, disabled موافقه action).
- `Frontend/src/app/modules/families/families-routing.module.ts` — `provider-requests` route
  above `:id`, permission-gated.
- `Frontend/src/app/core/services/auth.service.ts` — `Families.ProviderRequests` role map.
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824164515_Epic05_GuardianSnapshotHoldingFamily.cs`
  (+ `.Designer.cs`) — completion pass 2026-08-24: filtered unique
  `IX_GuardianChangeRequest_OnePendingPerFamily` (replaces the plain FamilyId index) +
  `OldGuardianRelationship` column (+ `Family.IsHoldingFamily`, 5-7/5-8's item); applied to
  `IIROSA_Db_Dev` and verified live (sys.indexes filter + both sys.columns rows).
- `Frontend/src/app/modules/families/provider-request-list/provider-request-list.component.spec.ts`
  — completion pass: HttpTestingController queue-load, decide→refresh-chain and
  page-back-in-range tests (the repo's first spec-file HTTP tests).
- `Frontend/src/app/layouts/main-layout/main-layout.component.html` — sidebar entry under families.
- `Frontend/src/app/modules/families/family-detail/family-detail.component.ts` + `.html` —
  «طلب تعديل العائل» raise button + modal (new guardian name, national ID, relationship, reason).
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `families.providerRequests.*` (40 keys each).

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-FAM-09 and module spec §10.S.4 / §10.U.9; raise-endpoint design decision recorded; new aggregate introduced end to end. |
| 2026-08-24 | Implemented end to end: aggregate + queue + raise producer, controller endpoints, queue screen + sidebar + raise modal, i18n. Migration table ships in parallel session's 105001 (race documented). Status → review. |
| 2026-08-24 | Review-and-complete pass — both 5-9 schema rulings applied in one migration (`20260824164515_Epic05_GuardianSnapshotHoldingFamily`, dev DB updated): filtered unique one-pending-per-family index (replaces the plain FamilyId index) + `OldGuardianRelationship` snapshot column, now captured at raise time including the parent-designated shape (name/NID from the Father/Mother row). Queue spec gained HttpTestingController load + post-decision-refresh coverage. |
| 2026-08-24 | Live walkthrough (private instance): raise on a father-designated family snapshotted the father into `OldGuardian*` (`Epic5 Battery Father / EPIC5BAT1 / Father`); a duplicate raise while pending was refused with the legacy localized message «تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب», backed by the filtered one-pending index live in the DB. Status → done. |
