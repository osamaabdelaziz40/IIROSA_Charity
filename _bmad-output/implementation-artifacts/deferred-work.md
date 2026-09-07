## Deferred from: code review of 3-1-list-all-charities and 3-2-verify-charity-name-availability (2026-08-19)

- **Tenancy claims have no revocation path.** Moving a user between charities has no effect until
  their access token expires (30 minutes). Inherent to stateless JWT; would need a token-version
  claim or a server-side revocation list.
- **No tests for a security-boundary change.** `ApplyCallerScope` has four distinct branches
  (charity claim, country claim, neither, unauthenticated) and zero tests. `Backend/tests/` holds no
  test project. Deferred by explicit user decision on 2026-08-19.
- **Validation and normalisation sit in the controller** rather than a FluentValidation validator in
  the service layer. The charity validator is scoped to stories 3-3 and 3-5.
- **Server-side response strings are hard-coded English.** Applies to every controller in the
  project, not just the new endpoint.
- **`CharitiesController` inherits `ControllerBase`, not `ApiController`.** Pre-existing in 20 of 21
  controllers; fixing one in isolation makes it inconsistent with its neighbours.
- **`text-input` component breaks the mandated 4-file shape** — `.css` rather than `.scss`, and no
  `.spec.ts`, so the new `nameTaken` branch ships with no coverage.
- **`catchError(this.handleError)` is passed unbound** in `charity.service.ts`. `handleError` is a
  plain private method used this way by roughly 25 methods; either fine everywhere or broken
  everywhere.
- **`ngx-bootstrap@^12` is incompatible with Angular 18.** `npm install` fails with `ERESOLVE`;
  every developer and CI job currently needs `--legacy-peer-deps`. Needs an upgrade to v18.

### Deferred by decision during the same review

- **`ApiResponse<T>` envelope is not used by `CharitiesController`.** CLAUDE.md requires every
  endpoint to return `Framework.Core.ApiResponse`/`ApiResponse<T>`, but only 1 of 21 controllers
  does. Converting a single endpoint would make its own controller internally inconsistent and
  break any client that reads its neighbours. This wants one migration across all controllers,
  scoped as its own story, not a per-story fix.

- **No unique index on `Charity.Name`.** `CharityConfiguration` has `HasIndex(x => x.Name)` without
  `.IsUnique()`, unlike `Code`. Two concurrent creates of the same name both pass
  `IsNameUniqueAsync` and both insert. Adding the constraint needs a data check first, because the
  migration fails if duplicates already exist. Find them with:

  ```sql
  SELECT LTRIM(RTRIM(Name)) AS Name, COUNT(*) AS Copies
  FROM [IIROSA].[Charity]
  WHERE IsDeleted = 0
  GROUP BY LTRIM(RTRIM(Name))
  HAVING COUNT(*) > 1;
  ```

  If that returns nothing, the index can be added safely. Note the application now normalises
  names on save, so new leading/trailing-space duplicates can no longer be created.

## Deferred from: epic-14 review-and-complete pass (2026-08-23)

The Technical Support frontend module was wired to a phantom base path
(`/api/technicalsupport`) with FormData bodies, PATCH verbs and envelope-typed responses — every
call 404'd. The pass rewired the whole module to `/api/SupportTickets`. What remains:

- **No attachment endpoints on the controller.** `SupportTicketService.AttachFileToTicketAsync`
  exists but no controller action exposes it; there is no download action either. The create
  form's file picker validates and previews client-side then silently drops the file; the detail
  screen shows attachment names without download links. Needs a multipart action + a
  `GET {id}/attachment` and frontend wiring (the shared `attachment` component is the mandated
  UI once it exists).
- **Assign/unassign.** Backend `POST {id}/assign` binds `[FromBody] string` (a raw JSON string
  body — `ApiService` cannot send it without content-type gymnastics) and there is **no**
  unassign endpoint at all. The UI now shows assignment read-only instead of faking it. Fix wants
  a proper `{ assignedToUserId }` DTO on the backend plus a user-picker on the frontend.
- ~~**Server-side sorting is a no-op.**~~ **RESOLVED 2026-08-23 (epic-14 code review, P11):** the
  repository now has an `ApplySorting` (title/createdOn/updatedOn/priority/status, `asc`/`desc`/`ascending`),
  the service forwards the filter's search + sort params, and the list UI only marks columns
  sortable that the backend actually orders by.
- **`ISupportTicketRepository.GetByCodeAsync` throws `NotImplementedException`** (pre-existing,
  by design of the copy) — no caller hits it today.
- **Board note corrected:** 14-6's endpoint comment said `GET /api/SupportTickets/report`; the
  action is `POST` with `{startDate, endDate}`.
- **Serializer fact for future wire work:** `Program.cs` chains `.AddNewtonsoftJson()` after
  `.AddJsonOptions(...)`, so **Newtonsoft serializes everything** — list endpoints return
  `{ items, totalCount }` (named anonymous object, not a ValueTuple), and trailing acronyms keep
  their case (`TicketsResolvedWithinSLA` → `ticketsResolvedWithinSLA`). The dead
  `AddJsonOptions` block (ReferenceHandler/WhenWritingNull) never takes effect.

## Deferred from: code review of epic-14 (2026-08-23)

- **Requester names render GUIDs.** `CreatedByUserName`/`CreatedByEmail` map straight from the
  raw `CreatedByUserId`, `AssignedToName` from `AssignedTo`, and the report's `TicketsByCreator`
  groups by user id — the list "Created By" column, the detail name/email fields and the report's
  by-creator table display 36-char GUIDs. Fix wants a Framework.Identity user-profile lookup/join
  in `SupportTicketService`/`SupportTicketMappingProfile`. (Pre-existing backend limitation
  surfaced by the epic-14 wiring; found by the 2026-08-23 code review.)

## Deferred from: code review of epic-15 (2026-08-24)

- **§20.S.1's 8th command (استخراج البيانات / export) is unimplemented and unowned.** The list
  screen's export command never shipped: 15-1 deferred it to "15-9 etc." and 15-9 closed without
  it. Needs a board assignment (a story of its own or an attachment to a reporting epic).
- **Hand-migration snapshot drift (missions).** `20260823154500_MissionAssigneeToIdentityUsers`
  was written by hand with no Designer/snapshot pair, so the next `dotnet ef migrations add` diffs
  against a stale snapshot and may emit a spurious `DropTable("ApplicationUser")` — the next
  migration author must hand-prune it. The junk `dbo.ApplicationUser` table also remains in the
  dev database.
- **LookupManagementController read-widening.** ~~Class-level auth lets any authenticated role read
  every catalogue and bulk-export any lookup table via `tables/{tableName}/export` /
  `tables/summary`~~ **RESOLVED 2026-08-25 (19-4):** export now `SuperAdminOnly`; summary kept
  readable by the minimal-break ruling (overview cards render for every role); catalogue reads stay
  global reference data by the 16-8 ruling.
- **Seed concurrency.** Parallel instance startups can race past the seeders' exist-guards and
  double-insert catalogue rows (dev-only exposure today).
- **Catalogue reads cap at `PageSize=1000`** ~~(mission-types, interview-types, employees select) —
  silently truncates past 1000 rows; a platform-wide pattern, not missions-specific.~~
  **RESOLVED 2026-08-25 (19-4):** `LookupFilterDto` clamps to a named ceiling (`MaxPageSize = 5000`),
  totalCount is the true filtered count on all three paged reads, and the 13 dropdown endpoints use
  the same ceiling via `DropdownPageSize` — truncation past the ceiling is now detectable, not silent.
- **No concurrency token on `Mission`** — an update and a register-result can race, last write
  wins. Rowversion is absent platform-wide; fixing one entity in isolation is inconsistent.

## Deferred from: code review of epic-4 (2026-08-24)

- **Whole-table materialization in the employee list** — `EmployeeService.GetEmployeesFilteredAsync`
  loads every Employee via `GetAllAsync()` then filters/paginates in LINQ-to-Objects; needs an
  IQueryable repository surface. Register-growth risk 4-1's defect list already raised.
- **Per-row sequential role fetch within the page** — bounded N+1: `GetEmployeeRolesAsync` per page
  row re-fetches the employee by id before the identity call; batch by `FK_UserId` when reworking.
- **Role filter materializes full role membership per request** — `GetUsersInRoles(role)` loads
  every user holding the role on each filtered list call.
- **`IsEmailUniqueAsync`/`IsCodeUniqueAsync` are not soft-delete-aware** (`EmployeeRepository.cs:53-75`)
  — a soft-deleted row's email/code still blocks reuse; semantics debatable while `DELETE`
  hard-deletes anyway.
- **`DELETE {id}` hard-deletes the Employee and orphans the identity user** — 4-6 adjacent hazard;
  needs its own decision/story (soft-delete + account cleanup).
- **Identity sync on employee deactivate/activate is advisory-only** — failures logged, not
  surfaced (4-6 defect 2); deliberate fail-vs-drift decision pending.
- **Repository `SaveChangesAsync` vs the IUnitOfWork-only rule** — module-wide systemic debt
  (4-3 defect 5); not fixable per-story.

## Deferred from: code review of epic-8 stories 8-1…8-11 (2026-08-24)

- **ExceptionMiddleware returns `exception.Message` verbatim in 500s** (no production masking) —
  pre-existing platform behavior; the epic-8 review stripped `error = ex.Message` only from its
  own new `FamiliesController` catches. [Backend/src/IIROSA.Api/Middleware/ExceptionMiddleware.cs:45]
- **Phone validation rejects Arabic-Indic digits (٠–٩)** — ASCII-only regex on an RTL-first app;
  needs a product/locale decision (normalize to ASCII on entry vs. accept-and-store).
  [Backend/src/IIROSA.Application/Validators/Family/OrphanCodingValidators.cs:83]
- **`LookupManagementController` class-level `SuperAdminOnly` → plain JWT auth with per-action
  policies** — change visible in the shared file during the epic-8 review but authored by a
  parallel session's uncommitted lookup work (it matches the epic-15 review's deferred
  recommendation above). That session must verify every write endpoint — bulk
  `tables/{tableName}/import` above all — carries a per-action policy before commit.
  [Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs]
- **`Orphan.Code` unique-index scope vs BR-07 per-charity uniqueness** (decision D1:c deferral) —
  `OrphanConfiguration.cs:60` declares `HasIndex(Code).IsUnique()` GLOBAL and unfiltered (carried
  by the applied migration), while BR-07 and the epic-8 service checks treat code uniqueness as
  per-charity (charity resolving via the orphan's own `FK_CharityId` or its family's). A code that
  is per-charity-available passes the service gate and then dies on the index — now surfaced as a
  clash-400 via the new `DbUpdateException` catch on the assign endpoint, but the underlying
  disagreement stands. Decide: filtered/composite index `(FK_CharityId, Code)` + migration (note
  the charity can be null → family-resolved, so a plain composite needs that denormalised first),
  or widen the service checks to global and re-record BR-07 as global uniqueness.
  [Backend/src/IIROSA.Domain/Configurations/OrphanConfiguration.cs:60; FamilyService.cs AssignOrphanCodeAsync]

## Deferred from: code review of epic 16 (2026-08-24)

- Year immutability vs date-year divergence on letter update — stored Year never re-derives when the
  Date's year changes (documented design protecting the serial anchor; revisit only if the business
  wants serial re-anchoring). [IncomingService/OutgoingService UpdateAsync]
- Employee keyspace split — incoming form's الموظف المسئول validates against the `Employee` table
  (`IncomingService.cs:420`) while the 16-9 attachment screen serves `ApplicationUser` ids
  (`IncomingEmployeeRepository.cs:47`); each path is internally consistent with its own spec text —
  product-level decision needed on which set "employee" means.
- Attachment download via plain `<a href="/api/attachments/{id}/download">` cannot send the JWT
  Authorization header — platform-wide shared-attachment pattern, not an epic-16 regression; needs a
  platform answer (cookie auth or blob fetch).
- Stale permission identifiers `IncomingOutgoing.Import/Export/ViewHistory` retired with the export
  wizard — verify no permission seed still lists them and prune.
- Reply-to letter dropdowns cap at `pageSize: 1000` — switch to searchable/typeahead select (ng-select
  already in the stack) before legacy data import pushes a charity past 1000 letters.
- Unattached-orphans grid materializes every link id in memory, and extract-all sets
  `pageSize = totalCount` unbounded — perf hardening (server-side paging or a dedicated export read).
- Four competing serial-padding implementations (backend D4, frontend pad, slice(-4), display pipe) —
  consolidate to one helper.
- New components `incoming-employees`, `outgoing-orphans`, `outgoing-orphans-report` lack `.spec.ts`
  and `ChangeDetectionStrategy.OnPush` — tests excluded per the standing decision; OnPush is a
  follow-up consistency pass over the whole module.
- Unbounded `pageNumber`/`pageSize` on correspondence reads — clamp (`Math.Clamp(pageSize, 1, 500)`)
  must exempt the by-design extract-all path, so it needs a dedicated export read first.
- Outgoing Excel extract omits some grid columns — export fidelity polish.

## Deferred from: code review of epic-17 backend chunk (2026-08-24)

- **Soft-delete filtering absent platform-wide** — `Framework.Core`'s `SetGlobalQueryForSoftDelete`
  is dead code (never invoked; zero `HasQueryFilter` in the `ApplicationDbContext` model
  snapshot). Every module's reads run unfiltered; epic-17's story claims of a "global query
  filter" misstate the runtime. Latent until any delete path lands (epic 17 ships none).
  Platform decision needed: enable the filter centrally (changes every module's queries — audit
  for code that deliberately reads soft-deleted rows first) or mandate per-repo filtering.
- **Σ-lines-≤-header TOCTOU** — `HqTransferService.SaveTransferDetailLineAsync`'s in-memory sum
  check has no concurrency control and no DB constraint backs it; two parallel line saves can
  both pass and land Σ above the header amount, both returning 200. Needs a platform-level
  strategy (CHECK/trigger, serializable transaction, or rowversion); low exposure — internal
  admin module, single-digit concurrent writers.
- **Module controllers on `ControllerBase`** — OfficeProjects, Missions, SeasonalAid, SupportTickets
  et al. skip the platform `ApiController` base that CLAUDE.md mandates (`AuthController` and
  `PeriodicOrphanReportsController` follow it). Epic 17's own controller is being fixed in this
  review; aligning the older modules is platform-wide cleanup (mirrors the 3-1/3-2 deferral of
  the same class of issue).

## Deferred from: live verification of epic-8 (2026-08-24)

- **`GetFamiliesAsync` leaks soft-deleted families — observed live.** The base family list
  (`FamilyService.GetFamiliesAsync`, query built at ~L892 from `IncludeNavigationProperties()`)
  applies charity/search/familyType/country/city/provider/count/date/isActive filters but **never
  `!f.IsDeleted`** — while `DeleteFamilyAsync` (~L1088) sets `IsActive = false` **and**
  `IsDeleted = true`. Verified against the running API: after soft-deleting the epic-8
  verification seed families, `GET /api/Families` still returned all three rows (`totalCount: 3`).
  First *observed* instance of the platform-wide deferral recorded in the epic-17 review above;
  the epic-8 queries themselves filter correctly (orphan search was re-verified live post-delete —
  soft-deleted family's orphan vanished). One-line fix (`query = query.Where(f => !f.IsDeleted);`
  after the query seed) + a check for other FamilyService list paths that share the pattern —
  belongs to the families module (epic 2), not epic-8, so not patched in this review.

## Deferred from: code review of epic 7 stories 7-1..7-4 (2026-08-24)

Real findings verified in code but owned by other epics / pre-existing — NOT patched in this review. Raw evidence: `review-artifacts/findings-*` alongside `epic7-review-report.md`.

**Backend — other epics (in shared files epic 7 also touched):**
- [epic 18 / seasonal aid] `SetFamilyReceivedFlag` has no tenancy enforcement — service receives neither charity id nor role; any Charity caller flips the flag on any family. [FamiliesController.cs:~1304]
- [epic 6 / housing] `familyType=Housing` via generic `POST /api/Families` bypasses building/flat allocation validation entirely.
- [epic 6 / housing] `UpdateHousingFamilyAsync`: omitted `orphans` node mass-soft-deletes all children; sync never adjusts `OrphansCount`/`FamilyMembersCount`; an actively sponsored child can be soft-deleted by the edit sync. [FamilyService.cs:465-497]
- [epic 6 / housing] `FamilyRepository.IncludeNavigationProperties` lacks `Provider`, `HousingBuilding`, `HousingFlat` — guardian never listed in housing beneficiaries; `HousingBuildingName`/`HousingFlatName` always null in the housing detail DTO.
- [epic 5 / member control] Attach moves (action 1) write no audit record; `TargetFamilyCode!` null-safety rests on the validator; counter updates are read-modify-write (concurrent moves lose updates); attach to a deactivated target family allowed; actively-sponsored orphan movable. [FamilyService.cs:1370-1410]
- [epic 5 / orphan coding] Eligibility check: HQ caller passing FamilyId without CharityId compares non-null Guid to null → every family reports `familyNotFound`. Phone-duplicate check counts soft-deleted orphans. BR-07 code uniqueness is a check-then-save race (and a DB unique index would conflict with the soft-delete code-release promise). `OrphanRepository` charity filter misses orphans whose `FK_CharityId` is null but whose family is in-charity. [FamilyService.cs:2268-2507]
- [epic 5 / transfer] `FromCharityId = fromCharityId ?? Guid.Empty` sentinel; cascade touches only Family+Orphans (father/mother/provider/relative charity columns left behind); active sponsorships unguarded across the move.
- [pre-existing / house pattern] `ex.Message` returned in 500 bodies across FamiliesController (incl. the two new provider endpoints); mixed response envelopes (ApiResponse vs anonymous shapes) — the families raw envelope is a recorded 15-1 deviation.
- [cross-module] All new lookup catalogue GETs hard-cap `PageSize = 1000` and silently truncate.

**Frontend / i18n — concurrent epic sessions (epics 6/8/9/16/18) introduced these in shared files; flagged for their sessions:**
- Menu role drift vs `PERMISSION_ROLES`: Office Development Projects and Missions dropdowns widened to `Charity` while `*.View` maps stay `['SuperAdmin','Admin']` — dead menu entries for Charity users; stale "(Admin/SuperAdmin only)" comments left in place. Orphan Payments menu opened to Charity but the list endpoint 403s Charity without orphanId.
- `missions.missionDetails` key deleted from BOTH ar.json and en.json while three mission templates + a route pageTitle still consume it — live raw-key break.
- `userManagement.noRolesAvailable` deleted from ar.json only (kept in en.json) — Arabic-only gap.
- `technicalSupport.title` duplicate-key rename changes the resolved value (last-occurrence-wins before) — consumers of the old winning value regress.
- Login: 200-without-token path clears auth data but completes the observable success-side — subscriber cannot distinguish failed login.
- Mass i18n key deletions (generalChecks void workflow, incomingOutgoing ~90 keys, housingProjects vocabulary) with no migration evidence for referencing templates; en.json deletes the top-level `validation` section with no ar.json counterpart; employees-section ar/en additions grossly asymmetric.
- Assorted i18n quality: `generalChecks.isDone: "CheckDone"` token as Arabic value; "Operation Faild:" typo in both languages; "إيذون" (correct: إيصال) ×3; "موافه" (correct: موافقة); `hqTransfers` naming drift (duplicate parent/child labels, isExecuted/addLine mismatches); `housingProjects.form.child.fullName` labelled "First name"; single-key `periodicReviews` namespace with reviewer/approver mismatch.
- `lookup-management.service.ts`: `getHousingFlats(NaN)` throws synchronously bypassing `catchError`; `getRefuseReasons` filed under the UC-HOU-03 section header; `CountryDto.maxTransferAmount` read-only dead field.
- `main-layout.component.ts` removes ApexCharts global scripts while legacy `CustomJsCodes1.js`/`jquery.sparkline` still load (unverified claim that the npm wrapper covers all consumers).
- Refugee screens (epic 7 low-priority, non-blocking): no dirty-state guard on the four-section form; catalogue dropdown reads truncate at pageSize 500/1000; spec suite asserts untranslated key literals and overrides OnPush.

- (epic-7 review follow-up) `npm test` is not runnable in this workspace at all — no `test`
  architect target in `angular.json`, no karma.conf/src/test.ts, no jasmine/karma devDependencies.
  Spec files are compile-verified only (`tsc -p tsconfig.spec.json`). Wiring a real runner is a
  workspace-level task (touches shared angular.json/package.json owned by multiple sessions).
- (epic-7 review follow-up) `Lookup.HealthStatus` is empty in dev — the new §12.S.2 health-status
  selects on the refugee form render empty until statuses are seeded via lookup management.

## Deferred from: code review of epic 5 stories 5-6…5-14 (2026-08-24)

- **[5-6…5-14] English-only server messages on an Arabic-first platform** — every BusinessException/validator/outcome string in the epic-5 surfaces is hard-coded English (consistent with the whole codebase); wants a platform-level server-message localization scheme (keys + ar/en), not per-story fixes.
- **[5-6] Soft-deleted orphans excluded from the transfer cascade** — `GetByFamilyIdAsync` filters `!IsDeleted`, so deleted orphans keep the old charity's `FK_CharityId` forever; benign while deletion is terminal, resurfaces with any un-delete/history feature.
- **[5-6, 5-7/5-8, 5-9/5-10] No concurrency tokens platform-wide** — Family/Orphan/Provider/GuardianChangeRequest carry no rowversion; concurrent transfers/moves/decisions are last-write-wins (corrupted transfer trail, phantom holding families, approve+reject both landing). Rowversion is absent platform-wide (same deferral as epic-15's Mission); fixing one entity in isolation is inconsistent — one platform decision + sweep. Minimal in-transaction re-checks are patched per-story in this review.
- **[5-7] `GenerateFamilyCodeAsync` check-then-insert race** — concurrent detaches can draw the same `FAM-yyyy-nnnn` code (no unique index verified); folds into the concurrency/token decision above.
- **[5-7/5-8] OnPush for the family-members component** — module siblings are all default-CD; flip the whole families module in one consistency pass (families module already deviates from the CLAUDE.md rule module-wide).
- **[5-8] `FamiliesController` stays on `ControllerBase`, not the platform `ApiController` base** — pre-existing file idiom; mirrors the 3-1/3-2 and epic-17 deferrals of the same class; platform-wide cleanup story.
- **[5-9] GET queue returns bare `{items, totalCount}`, not the ApiResponse envelope** — recorded module ruling (matches `GetFamilies`); normalize the families module's GETs in one sweep to avoid a wire break per-endpoint.
- **[5-9] Charity-filter dropdowns cap at one page of 500 with no loading/error state** — module-wide dropdown pattern (transfer modal, queue filter); switch to a searchable/typeahead select (ng-select already in the stack) before the register passes 500 charities.
- **[5-9] §10.S.4 `GetNext()` record-navigation command unimplemented** — spec-to-screen gap outside story scope; backlog candidate.
- **[5-12] Provider attach path validates via `[Required]` DataAnnotations + controller ModelState** — not the service-layer FluentValidation convention; pre-existing DTO shape, normalize with the platform validator sweep.
- **[5-13] Service-level Charity-role refusal maps to 400 rather than 403** — cosmetic (the endpoint already 403s); align with the middleware mapping when the FamiliesController catch-ladder is reworked.
- **[5-11/5-14] Live walkthrough deferred** — the running API predates the unapplied migration tail; all runtime ACs for the epic-5 stories rest on static verification until the deployment window (disclosed in each story's Dev Agent Record).

## Deferred from: code review of epic 6 (2026-08-24)

- **[6-*] `SearchOrphansAsync` returns other charities' families for a claim-less Charity token** [Backend/src/IIROSA.Application/Services/FamilyService.cs:1600] — orphan-eligibility scoping is epic-8 territory (UC-ORP-05); pre-existing, deferred.
- **[6-*] `CheckOrphanCodeUniqueAsync` leaks code-existence across charities** [FamilyService.cs:1780] — uniqueness scope is epic 8's rule to land; deferred with it.
- **[6-*] HQ orphan-eligibility check answers `familyNotFound` even when the family exists but the orphan link differs** [FamilyService.cs:1700] — same epic-8 orphan-eligibility cluster.
- **[6-3] `TargetFamilyCode` NRE when the transfer target code is null/blank** [FamilyService.cs:1560] — epic-5 transfer path; pre-existing, deferred.
- **[6-6] Transferring a family strands its periodic-report history (reports keep the old CharityId)** [FamilyService.cs:1393] — cross-module design with 5-6 transfer + report scoping; also recorded inline in 6-6.
- **[6-*] Lookup catalogue reads page at `PageSize = 1000` and silently truncate** — ~~platform-wide lookup pattern, not epic-6 code; normalize in the lookup sweep.~~ **RESOLVED 2026-08-25 (19-4).**
- **[6-8] `flagFinishedSponsorship` renders «العمر» in `ar.json`** — foreign hunk landed from a parallel session inside the epic-9/18 i18n block; fix with that block's owner, not from epic 6.

## Deferred from: code review of epic 17 (2026-08-24)

- **[17-1] `HqTransferDetail` + `Country.MaxTransferAmount` creation rides in the Epic06-named
  migration `20260824105001_Epic06_RetireConstructionHousing.cs`** — documented parallel-session
  bundling (that migration's header says so). Only bites an environment updated to a partial chain
  (Epic17 checkpoint without the Epic06 migration → UC-TRF-08 fails with "Invalid object name
  HqTransferDetail"). Revisit only if environments get partial chains.
- **[auth] `AuthService.login` failed-token branch falls through into the success path** — the
  new `if (response.token) … else clearAuthData()` hunk (a login-hardening change from a parallel
  session sharing the file, not epic 17's) has no `return`/`throw` after `clearAuthData()`, so
  execution continues down the pre-existing "verify token was stored" flow with nothing persisted.
  Real observation worth the owning session's review; flagged here so it isn't lost.

## Deferred from: code review of epic 9 (2026-08-24)

- F1 Concurrent reviews last-write-wins (no RowVersion) — schema + concurrency work beyond review scope [PeriodicOrphanReportService]
- F2 Detail extract runs on every GenerateReport — perf refactor, unmeasured impact [OrphanReportService]
- F3 Same-date supersede semantics in non-renewed window — minor spec ambiguity [ReportService]
- F4 API-caller inverted date range silently empty (UI already validates) [OrphanReportService/ReportService]
- F5 HQ charity dropdowns cap at 500 silently — tenant-scale dependent [orphan-reports-list/generate]
- F6 POR ReportNo nvarchar(max), count+1 race, D4 rollover — numbering scheme redesign
- F7 9-13 reason column red styling — cosmetic (AC met)
- F8 andOr always sent on wire — harmless no-op without status predicates [orphan-report-search]

## Deferred from: code review D2 decisions (2026-08-24)

- Main-layout menu defects — Missions + OfficeDevProjects shown to Charity vs permission map; incoming/outgoing labels crossed + import/history entries removed; HQ-transfers duplicate label; families create-link ungated [main-layout.component.html] — co-owned, deferred to owning sessions
- auth.service login fall-through on 200-without-token (navigates then bounces, no error message) [auth.service.ts:182-230] — co-owned, deferred to owning sessions
- ReportService: missing-files coded-only predicate omitted; DateDiffYear age overstate; beneficiary-families unbounded materialization; HQ narrow bypasses country pin; hard-coded English error fallback; naked union return on getFamilyEntryTracking [ReportService.cs / report.service.ts] — co-owned, deferred to owning sessions

## Deferred from: 19-4 lookup sweep (2026-08-25)

Page-cap normalization and the bulk-export gate landed (see the resolved entries above). What stays
deferred, with owners:

- **Dropdown → searchable/typeahead select migration.** The ceiling (5000) is a detectable signal,
  not a fix for outgrowing catalogues: any dropdown that genuinely approaches the ceiling wants an
  ng-select typeahead instead of get-all lists. Belongs to each owning screen (epic-16's
  reply-to-letter dropdowns, epic-9's HQ charity dropdowns, 5-9's transfer/queue filters — their
  existing deferral entries stand).
- **`LookupManagementManagementService.cs` duplicate-pair debt** — the summary/export service lives
  in a `*ManagementManagement` file whose class is named `LookupManagementService` (CLAUDE.md known
  debt); consolidation is a platform cleanup, untouched by 19-4.
- **`ImportLookupTableAsync` is a `NotImplementedException` stub** — no controller action exposes
  it (verified in the 19-4 write-action audit); the bulk-import UC-14.15 story owns any real
  implementation. If it ever gets an action, it must carry `SuperAdminOnly` like every other write
  (19-4 audit: all 17 live write actions gated).
- **Summary covers only the 8 core tables** — `tables/summary` omits the epic-7/8/9 catalogue
  family (education-levels, health-statuses, refuse-reasons, refugee set…), so the overview cannot
  surface the epic-7 "HealthStatus renders empty" gap as a 0-card. Extending the summary list wants
  its own small story (needs each table's repository injected — the service's ctor is already wide).

## Deferred from: 19-11 country NID rules (2026-08-25)

- The family form (families/family-form) has four NID controls (father/mother/provider/relative) but NO country selector — NID rule validation cannot be wired there until a country source exists on that form (EP-05 scope). The refugee form is wired; nationalIdRuleValidator is exported from refugee-family-form.component.ts for adoption.
- The lookup-management countries admin screen does not render the two new rule columns (maintainable via API/seed); add to the screen if HQ asks for UI maintenance.

## Deferred from: code review of epic 18, all stories (2026-08-25)

Full triage: `_bmad-output/implementation-artifacts/review-artifacts/epic18/triage.md` (6 decision-needed, 27 patch, 7 defer, 3 dismissed; raw findings in the sibling `findings-*` files).

- **[DF1] Guardian-change report semantics undeliverable** — prior-guardian columns always null (no Provider audit trail), `ChangeDate` synthesized from `UpdatedOn>CreatedOn`, two different widow discriminators between ReportService and ReportSheetService. Needs a Provider audit-trail feature, not a patch. [ReportService.cs GetProviderSponsorChangesAsync]
- **[DF2] Recorded UC limitation bundle** — UC-RPT-20 printed/disbursed/confirmed columns omitted; UC-RPT-22 reason always null; UC-RPT-38 v1 reports only zero-attachment letters; UC-RPT-29 stopped counts as not-received + mixed-currency batch reports first currency. Each needs new persisted state or a spec revision.
- **[DF3] Structural debt in report core** — `AttachmentService` injected as concrete class; 40-parameter `ReportService` constructor (the mechanism behind silent validator non-wiring); four coexisting export paths in `report-export.service.ts`; cross-module type imports contradicting the shim rule; ~15 drifted scope-ladder copies (root cause of the fail-open patch — consolidate when touching it).
- **[DF4] Charity entity lacks NameAr/NameEn** — report output cannot be bilingual on the charity axis; schema + migration + data fill (entity-level debt surfaced into report output).
- **[DF5] UC-DSH-01/02 (dashboard summary/charts) unimplemented** — DashboardController hosts only payment-summary; out of epic-18 scope, scope note for the parent epic.
- **[DF6] Zero test execution across epic 18** — standing user decision; all 26 specs are should-create stubs (the one real spec is compile-dead and is being patched). Resurfaced for the record, not re-litigated.
- **[DF7] Data-limited live evidence in 18-31/18-32** — zero disbursement batches in the dev DB (card sheet never rendered live; dashboard figures cross-checked statically); 18-2's smoke was batched into a group run. Re-smoke when a populated DB exists.

Related cross-epic findings surfaced by this review (decision D4 in the triage — fix-now-vs-handoff pending): PeriodicOrphanReportService self-approve hole, FamilyService data-destruction/tenancy bundle, ArabicAmountInWords ≥10^12 crash. Several FamilyService items overlap the epic-7 deferral entry above.

## Deferred from: epic 19, story 19-13 (2026-08-25)

- **[DF-19.13a] NLog-not-log4net deviation recorded** — module spec §24 (WAR.IIROSA legacy) references log4net; the platform logs via **NLog** (`nlog.config`, `builder.Host.UseNLog()`). Deviation is documentation-only: no logging package change was made or is needed. Server-side full-detail logging (message/stack/inner) verified intact for every status path (AC 3).
- **[DF-19.13b] Dev proxy defeated by environment.ts** — `Frontend/src/environments/environment.ts` hard-codes `apiUrl: 'https://localhost:60960'` although its own comment says the proxy handles the /api prefix; every dev-server session bypasses `proxy.conf.json` and talks to the live API directly. Surfaced during the 19-13 browser verification (the "kill the API" test landed on the error page because the live API was down, not because the proxy was cut). Fix belongs to a dev-experience story: empty the dev `apiUrl` and route through the proxy.
- **[DF-19.13c] Environment-discrepancy guard note** — `IIROSA_Db_Dev` at verification time held 9 families of which 7 were soft-deleted by a parallel session; only `C172DFD4` (charity A, housing) and `758CB345` (charity B) were live. Any future dedup/clash battery must mine fixtures from live families (family `IsDeleted=0`), not just live holder rows — recorded after the 19-12 first-pass false alarm.

## Deferred from: code review of epic-19 (2026-08-26)

Findings triaged during the full epic-19 code review (all 13 stories). Items marked **parallel
session** were observed in uncommitted work outside epic-19's File Lists — left for the owning
sessions/epics; do not "fix" them from epic-19.

- [epic-19 · 19-1] System.Drawing (GDI+) image handling is Windows-only and the stored
  contentType is client-supplied — framework posture; the host is Windows and the extension
  allow-list is the real gate. Revisit on any Linux-container move. [AttachmentService.cs /
  AttachmentsController.cs]
- [epic-19 seeds] Seeders (Steps 9-11: guardian lookups, banks, NID rules) are AnyAsync-guarded
  but not concurrency-safe under simultaneous first boots — acceptable for dev bootstrap; make
  idempotent-with-lock if production seeding ever matters. [IIROSASeedDataInitializer.cs]
- [parallel session · epic-5/7] `ex.Message` leaks to clients in the `GetProvider` /
  `UpdateProvider` 500 bodies. [FamiliesController.cs]
- [parallel session · epic-7] `extractFieldErrors` (refugee form) drops server messages that are
  not field-keyed — non-field messages never reach the user. [refugee-family-form.component.ts:820]
- [parallel session · epic-7] Refugee `saveEdit` retry after failure can duplicate members —
  non-atomic client retry. [refugee-family-form.component.ts]
- [parallel session · epic-8/10] `GetBatchNumbers` charityId pinning is inconsistent with the
  sibling orphan-payment scoping. [OrphanPaymentsController.cs]
- [parallel session · housing] Flat-exclusivity check-then-act race between concurrent edits (no
  row-level guard on the assignment read-modify-write).
- [cross-cutting] `DateTime.Now` / `DateTime.UtcNow` are mixed in the day-report paths — day
  boundaries corrupt under a non-local server timezone; normalise to UtcNow (+ display offset)
  with the reporting vertical. (Location specifics unverified — reported by the review's
  adversarial layer; confirm before acting.)
- [parallel session · epic-5] Guardian-approve action's role set does not match the
  guardian-change writer set, and the approve flow falls back to a name-less label.
  [FamiliesController guardian endpoints]
- [epic-19 · 19-7] Region→centre cascade lacks stale-response guarding (rapid parent switches
  can pin the previous region's centres). [refugee-family-form.component.ts:200-202;
  charity-form cascade shares the idiom]
- [parallel session · epic-5/8] `deletedOrphanIds` is not pruned when the orphan-coding form
  retries after a failure — stale deletions ride the retry. [orphan coding form]
- [parallel session · epic-16] `IncomingDto` pagination is unclamped (negative/huge values pass
  through). [IncomingOutgoing DTOs]
- [parallel session · epic-16] Cheque `serial` parsed with a bare `Number()` — NaN reaches the
  wire on malformed input. [cheque screens]
- [epic-19 · 19-8] ~15 `debugger;` statements remain in charity-form.component.ts (:414-878) —
  devtools pauses hit production users; strip with the owning screen's next change.

## Epic-18 code review (2026-08-26) — deferrals and handoffs

Step-04 execution re-scopes (triage DF* items stand unchanged; full execution record in
review-artifacts/epic18/triage.md):

- [epic-18 review · P13] Unbounded `ToListAsync()` before in-memory group/paging in
  GetMissedPayments / GetBeneficiaryFamilies / GetFollowUpActivity — the EF-safe server-side
  rewrite risks GroupBy/string-join translation breakage and the epic shipped without the test
  harness running (standing decision); pre-authorized defer at triage. [ReportService.cs,
  FamilyService.cs]
- [epic-18 review · P16] Hard-coded English user-facing strings in ReportService /
  ReportSheetService (batch-not-found, validator messages, sheet titles,
  Relationship="Mother") — moved out of the patch per the standing platform ruling on
  server-side localized resources; centralize when the platform picks a message catalog.
  [ReportService.cs, ReportSheetService.cs]
- [epic-18 review · P17] 18-13 `GetFamilyFollowUpAsync` untyped `Task<object>` + raw envelope —
  re-typing is a wire-shape change that requires the frontend co-change; ride it with the D5
  batch-key naming alignment. [FamilyService.cs, FamiliesController.cs]
- [epic-18 review · D4 · owning vertical] No Blacklist/banned-person entity exists in the
  Domain — the WAR.IIROSA blacklist gate cannot be implemented until the owning vertical lands
  the entity; FamilyService create/update gates then key on it. [IIROSA.Domain, FamilyService.cs]
- [epic-18 review · D4 · epics 4/8/9] FamilyService data-semantics bundle: absence=soft-delete
  children sync destroys live orphans on a racing GET→PUT; HQ can mint charity-less Regular
  families invisible to every scoped read; eligibility/code-unique scopes steered by client
  input; audit actor defaults to literal "HQ"; report-number race surfaces as a user-retry 400;
  member-move leaves the sponsor link/unlink hole open. [FamilyService.cs]
- [epic-18 review · D4 · cheque epic] `ArabicAmountInWords` crashes ≥ 10^12 (`Scales[4]`) and
  overflows above long.MaxValue — zero tests. [ArabicAmountInWords.cs:153]

## Deferred from: code review of epic-10 stories 10-1..10-9 (2026-08-26)

- [epic-10 review · W1 · platform] `AuthService.hasPermission` fails OPEN for unmapped
  permission keys — pre-existing platform behavior; ~60 new keys now depend on it; harden
  fail-closed in core. [auth.service.ts]
- [epic-10 review · W2 · epic-10 regression] Enrolment write (POST {id}/orphans → added/
  skipped + BR-17 Amount snapshot) has zero execution evidence — dev DB holds no Orphan rows;
  must be exercised in the epic-10 final regression. [OrphanPaymentService.cs]
- [epic-10 review · W3 · 10-21/epic-18] Payment-summary band on the batch detail 403s for
  Accountant/FinancialOfficer/Charity (DashboardController payment-summary = SuperAdmin,Admin
  only) while the detail route admits five roles — align roles with the owning story.
  [DashboardController.cs:38-39]
- [epic-10 review · W4 · pre-existing] N+1 orphan-count loop (`GetOrphanCountAsync` per group)
  on both the list (OrphanPaymentService.cs:465) and orphan-history (:532) branches — batch
  into one GroupBy count. [OrphanPaymentService.cs]
- [epic-10 review · W5 · pre-existing] Batch-number generator: a manually stored prefix-sharing
  value (e.g. `BP-202608-9`) breaks int.TryParse → next-sequence falls back to 1 → duplicate →
  every create 500s for the rest of the month; no concurrency guard on simultaneous creates.
  [OrphanPaymentRepository.cs:45-68]
- [epic-10 review · W6 · 8-8 surface] Orphan-history branch ignores `filter.SortBy`, always
  orders by GroupDate. [OrphanPaymentService.cs:1082-1084]
- [epic-10 review · W7 · owning epics] Scope creep riding epic-10 files: PeriodicOrphanReport
  housing fields (epic-6, schema covered), detail print buttons + payment-summary band
  (10-21/10-24), OrphanRepository SocialStatus include (§12 refugee register), auth login
  token-guard hunk (epic-1/parallel session) — none acceptance-checked here; route to the
  owning epics' reviews.
