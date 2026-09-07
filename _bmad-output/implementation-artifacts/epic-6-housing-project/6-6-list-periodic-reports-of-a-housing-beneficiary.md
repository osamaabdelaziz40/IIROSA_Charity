# Story 6-6: List periodic reports of a housing beneficiary

| Field | Value |
| --- | --- |
| Story key | `6-6-list-periodic-reports-of-a-housing-beneficiary` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-06 — التقارير الدورية للأسر الساكنة |
| Priority / size | Could · 3 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.S.3 screen, §11.U.6 scenario) |
| Route | `#/housing-projects/:id/reports` (`:id` = housing family id) — screen status **planned** in §11.A |
| Endpoint | `GET /api/PeriodicOrphanReports/by-orphan/{beneficiaryId}?childOrParent=` |
| Depends on | 6-1, 6-3 (family exists); 6-7 resolves the beneficiary before this is usable end to end — build 6-7 next |
| Roles | `Charity`, `Admin`, `SuperAdmin` (endpoint also admits `Accountant`, `Employee` today) |

## Status

done

## Story

As a charity user, I want to be able to list periodic reports of a housing beneficiary التقارير
الدورية للأسر الساكنة, so that I can answer the operational, compliance or financial question
being asked of me.

## Acceptance Criteria

1. Given a charity user with an active session on the screen at `#/housing-projects/:id/reports`,
   when the actor opens the screen, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}` and the response is rendered on the
   screen without a page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §11.S.3 grid (الرقم · رقم التقرير · التاريخ · تم الاعتماد · تاريخ
الاعتماد) renders a housing beneficiary's report history — child OR guardian, selected by the
`ChildOrParent` discriminator — scoped to the caller's charity; empty grid + zero pages when no
report matches (§11.U.6 alternate flow).

## Reality check

The screen is **planned, not built** (§11.A route table): no `reports` routes exist in
`housing-projects-routing.module.ts` (verified — only list/create/detail/edit), no
`HousingReportListComponent` anywhere, and the `periodic-orphan-reports` module's routes are
NOT registered in `app-routing.module.ts` (the epic-9 audit exclusion) so nothing can be borrowed
from there by routing alone.

The endpoint half-exists: `GET /api/PeriodicOrphanReports/by-orphan/{orphanId}`
(`PeriodicOrphanReportsController.cs:258`, paged, roles incl. `Charity`) returns ANY orphan's
reports — it has **no `ChildOrParent` discriminator** (guardian reports are not representable),
and `PeriodicOrphanReportService` injects **no `ICurrentUserService`** — no charity/country
scoping on the read (AC 3/4 fail today). `CreatePeriodicOrphanReportDto` has no housing fields;
the entity design below amends 6-1's placement note: **the discriminator migration lands in THIS
story** (the read needs the column to filter on); 6-8 lands the create branch that writes
non-default values.

## Binding design: housing-beneficiary reports (decided here, consumed by 6-7/6-8)

`PeriodicOrphanReport` gains (migration `Epic06_HousingReportBeneficiary`):

| Column | Type | Meaning |
| --- | --- | --- |
| `ChildOrParent` | enum `ReportBeneficiaryType { Child = 1, Parent = 2 }` stored int, **default `Child`** (backfill: existing rows are child reports) | §11.U.6 "the ChildOrParent discriminator selects which" |
| `FK_HousingFamilyId` | Guid? | links the report to the housing family even when the subject is the guardian |

Read contract: `by-orphan/{beneficiaryId}?childOrParent=Child|Parent` — `Child` ⇒
`beneficiaryId` = orphan id (today's behaviour, back-compat for epic 9);
`Parent` ⇒ `beneficiaryId` = guardian/provider id of the housing family (match on the family
aggregate's guardian key — resolve via the 6-4 detail shape). Filter:
`ChildOrParent == requested` AND (for `Parent`) `FK_HousingFamilyId` family's charity == caller
scope. Unknown beneficiary/`childOrParent` → empty result, not 500.

## Tasks / Subtasks

- [x] **Task 1 — Domain + migration** (AC 2)
  - [x] `Enums/ReportBeneficiaryType.cs`; entity columns + navs per the table above;
        `PeriodicOrphanReportConfiguration` mappings (IIROSA schema, index on
        `(FK_HousingFamilyId, ChildOrParent)`); migration backfills `ChildOrParent = 1` —
        apply with the standard two-project `dotnet ef` form (live-API lock caveat: MSB3021/3027
        ≠ compile failure; never kill the user's process)
- [x] **Task 2 — Application: scoped read** (AC 2, 3, 4)
  - [x] `IPeriodicOrphanReportService.GetReportsByOrphanAsync` — extend with
        `ReportBeneficiaryType beneficiaryType` (default `Child`); implement the `Parent` branch
        per the design above
  - [x] Inject `ICurrentUserService` into `PeriodicOrphanReportService` and scope the read:
        caller with `CharityId` ⇒ only that charity's reports (pin-never-widen,
        `OfficeProjectService.cs:384` shape); HQ caller ⇒ all. Keep the existing
        `IsPeriodicReportsEnabledForCharityAsync` feature-flag behaviour on the write path (6-8)
  - [x] Result DTO carries `reportNo`, `reportDate`, `isAccepted/تم الاعتماد`,
        `approvalDate/تاريخ الاعتماد` for the grid
- [x] **Task 3 — API** (AC 2, 5)
  - [x] `by-orphan` action gains `[FromQuery] string? childOrParent` (parse; invalid → 400 with
        field message); roles unchanged (`SuperAdmin,Admin,Accountant,Employee,Charity`);
        raw envelope + `{ message }` catch — no `ApiResponse<T>`
- [x] **Task 4 — Frontend: the planned screen** (AC 1, 2)
  - [x] `housing-projects/housing-report-list/` 4-file component + route `:id/reports` in the
        module routing table (register BEFORE `:id` style conflicts — declare the more specific
        path first; the 11-9 `reconcile`-shadowed-by-`:id` bug class), guards
        `AuthGuard` + roles incl. `Charity`, `data.permission: 'HousingProjects.View'`
  - [x] Header: family context from the 6-4 read (family code/names), الجمعية drop-down
        (HQ-only) + ChildNameSearch box (§11.S.3's two filter fields) — beneficiary resolution
        is 6-7's lookup; until it lands, list defaults to the family's first child beneficiary
  - [x] Grid §11.S.3 order: الرقم (row serial formula — 13-1) · رقم التقرير · التاريخ ·
        تم الاعتماد · تاريخ الاعتماد; commands: add icon → `:id/reports/new` (6-8 wires save;
        render disabled until then), edit icon → `:id/reports/:reportId` (6-8), delete icon →
        confirm dialog + `DELETE /api/PeriodicOrphanReports` (9-6 endpoint — verify live before
        wiring; disabled if absent), print icons **render disabled** (epic 18 territory —
        `18-40` display, not this story), `Pagination` shared component for GetNext/GetPrev
  - [x] `OnPush`, `trackBy`, `takeUntil(destroy$)`; service method in
        `housing-project.service.ts` (or a small `housing-report.service.ts`) typed to the
        endpoint; camelCase reads (`items`, `totalCount`)
  - [x] i18n: `housingProjects.reports.*` keys in BOTH ar.json and en.json (grid headers,
        filters, empty state) — extend the 6-1 block; no hard-coded strings
- [x] **Task 5 — Verification** (AC 1–5)
  - [x] Live: seed one housing family + child report → `by-orphan/{orphanId}` lists it;
        `childOrParent=Parent` with a guardian id → guardian reports once 6-8 can create one
        (until then: empty result, 200); charity B token → 0 rows; invalid `childOrParent` → 400;
        unauthenticated → 401; epic-9 regression: `by-orphan/{orphanId}` without the param
        behaves exactly as before — **verified in the epic-6 review battery (35/35)**: child and
        guardian histories listed separately (B3/B4/B5), soft-deleted rows hidden, epic-9 legacy
        `by-orphan` regression green (Part A)
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

### Review Findings

Code review 2026-08-24 (blind+edge+auditor):

- [x] [Review][Patch] Report delete is a HARD row removal despite the "(soft)" log line (`DbSet.Remove`) — platform convention is IsDeleted soft delete; the module's own duplicate-slot comments assume soft-deleted rows exist [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:392] — **applied**: delete now soft-deletes (`IsDeleted` + base stamps); battery B4/B5 verified — deleted row disappears from reads while the child (orphan, month) slot stays held and the guardian slot frees
- [x] [Review][Patch] Report reads have no `!IsDeleted` filter anywhere (list, by-orphan, detail, export, GetScopedReport) — invisible today only because deletes are hard; once soft-deletes land, deleted reports reappear. Dup checks stay unfiltered BY DESIGN (soft-deleted row holds the (orphan, month) slot) [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:559] — **applied**: `!IsDeleted` on all read paths; dup checks left unfiltered by design (slot semantics verified live)
- [x] [Review][Patch] by-orphan history mixes guardian reports into the child's list — filter sets `OrphanId` but never `ChildOrParent`; guardian reports ride the carrier child's OrphanId; projection lacks the discriminator field too [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:559] — **applied**: read filters + projection carry `ChildOrParent`; battery B3/B4 verified the child list stays child-only
- [x] [Review][Patch] Routes omit the `data.permission` keys the stories name (`HousingProjects.View`/`Create`) — inert today (AuthGuard checks roles only) but spec'd; also covers 6-8's route [Frontend/src/app/modules/housing-projects/housing-projects-routing.module.ts] — **applied**: `canActivate: [AuthGuard, PermissionGuard]` + `permission:` key on all 6 module routes (`PERMISSION_ROLES` already carried `HousingProjects.*`)
- [x] [Review][Defer] Transferring a family strands its report history (reports keep old CharityId; new charity sees zero) — deferred, cross-module design with 5-6 transfer + report scoping; not fixable inside epic 6

## Dev Notes

### Platform rules that bind this story

- `:id/reports` route must be declared before any conflicting `:id`-catching route in the module
  table (11-9 defect class — specificity order).
- The `periodic-orphan-reports` feature module stays UNREGISTERED in `app-routing.module.ts`
  (epic-9 board exclusion) — do NOT register it from this story; the housing screen calls the
  API directly.
- Soft delete via the global query filter; only `IUnitOfWork` saves (this story writes nothing);
  camelCase wire; `NameAr ?? NameEn` labels; raw envelope.

### Out of scope

| Item | Story |
| --- | --- |
| Beneficiary code lookup populating this screen's search | 6-7 |
| Report create/edit form + `POST /api/PeriodicOrphanReports` housing branch + acceptance flags | 6-8 |
| Printing report forms | epic 18 (UC-RPT-40 / 9-17) |
| Registering the orphan-reports module routes | epic 9 |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.3] screen contract — 2 filter fields,
  5-column grid, 9 commands
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.6] scenario — ChildOrParent
  discriminator named
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-06 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs#L258] endpoint
  being extended
- [Source: Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs#L23] missing
  `ICurrentUserService` (scoping defect)
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-1-list-housing-families.md] epic entity design
  (amended here: discriminator migration lands in 6-6)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness)

### Debug Log References

- Migration repair: the first `ef migrations add --no-build` loaded a stale Infrastructure
  Efmig assembly (parallel-session rebuild race) and produced an **empty** migration
  (`20260824114636_Epic06_HousingReportBeneficiary`) while still rewriting the snapshot.
  Repair: `rm` the empty pair → full `dotnet build -c Efmig` → re-add
  (`20260824115014_Epic06_HousingReportBeneficiary`). Verified the regenerated Up carries
  EXACTLY four operations — AddColumn ChildOrParent int NOT NULL default 1 (the backfill),
  AddColumn FK_HousingFamilyId uniqueidentifier NULL, CreateIndex
  IX_PeriodicOrphanReport_FK_HousingFamilyId_ChildOrParent, AddForeignKey → Family ON DELETE
  SET NULL — plus the clean inverse Down; nothing from parallel sessions absorbed. The
  migration carries a `<remarks>` documenting the race and the always-full-build rule.
- Frontend build transiently failed in `families/refugee-family-*` (parallel session's
  in-flight edits, files minutes old); the second build was green with my changes and none
  of that breakage — not caused by 6-6.

### Completion Notes List

1. **Reality-check correction:** the story's scoping-defect list was stale — a parallel
   (epic-9) session had ALREADY landed `ICurrentUserService` scoping in
   `PeriodicOrphanReportService` (`ApplyCallerScope` on `GetPagedAsync`). 6-6 therefore adds
   the discriminator read ON TOP of the scoped path instead of re-adding scoping:
   `PeriodicOrphanReportFilterDto` gains `ChildOrParent` + `HousingFamilyId` (two additive
   predicates in `ApplyFilters`), and a new `GetHousingBeneficiaryReportsAsync` reuses
   `GetPagedAsync`'s projection rather than duplicating it.
2. **Interface shape deviation from Task 2's letter:** `GetReportsByOrphanAsync` was NOT
   re-signatured (that would touch every epic-9 caller); the housing read is a NEW method
   `GetHousingBeneficiaryReportsAsync(beneficiaryId, beneficiaryType, page, size)` on the same
   interface. Task 2's intent (discriminator-selectable read) is met; the epic-9 surface is
   source- and behaviour-compatible.
3. **Controller contract:** absent `childOrParent` → legacy `GetReportsByOrphanAsync`
   (epic-9 regression-safe); `Child`/`Parent` (OrdinalIgnoreCase) → the housing read; anything
   else → 400 `{ message, errors.childOrParent }`. Roles untouched. `Parent` resolves
   provider → family and 404s (NotFoundException, no existence leak) on unknown / deleted /
   non-Housing / out-of-scope; `Child` gates the orphan through the scope check like the
   legacy path.
4. **Entity semantics recorded for 6-8:** `OrphanId` stays the required carrier key on Parent
   reports too (the family's first child row carries the guardian report — default
   `ReportBeneficiaryType.Child` on the column makes pre-housing rows child reports by
   definition); `FK_HousingFamilyId` is the family link; ON DELETE SET NULL mirrors the rest
   of the aggregate; the composite index serves this screen's read path.
5. **Frontend screen:** `HousingReportListComponent` (§11.S.3) — family context from the 6-4
   read; الجمعية drop-down (HQ-only, CharityService) + child-name search box per the spec's
   two filter fields; PLUS an explicit يخص (Child/Parent) selector, because the legacy screen
   had no discriminator control while this story's DoD requires both branches reachable. Until
   6-7's code lookup lands, Child defaults to the family's first child (search narrows within
   the family's children); Parent targets the guardian's provider id.
6. **الجمعية semantics on a single-family dataset:** an HQ user choosing a charity the family
   does not belong to gets an explicit `charityOutOfScope` notice and an empty grid (the
   family is out of that charity's data by choice) — getCharityData()'s honest equivalent
   here, never a silent no-op control.
7. **Commands:** add/edit icons render disabled (6-8 wires `:id/reports/new` /
   `:id/reports/:reportId`); the three print commands collapse to one disabled printer icon
   (epic 18 — rendering three disabled clones of the same future action added nothing);
   delete is live: `notification.confirm` (نعم) → `DELETE /api/PeriodicOrphanReports/{id}`
   (epic-9 UC-ORR-06, verified present at `PeriodicOrphanReportsController` `HttpDelete`).
   The delete icon shows for SuperAdmin/Admin only (the endpoint's own roles) and disables on
   accepted rows (the service refuses accepted/locked). Charity users see no delete — server
   remains the enforcement point.
8. **Route + entry point:** `:id/reports` declared before `:id` (11-9 specificity class);
   entry is the new التقارير الدورية action on the 6-4 detail screen; breadcrumbs
   home → register → family code → reports. Serial column uses the 13-1 continuous formula;
   `app-pagination` supplies GetNext/GetPrev.
9. **i18n:** `housingProjects.reports.*` added to ar.json AND en.json (title, subtitle,
   filters, columns, commands, empty/noBeneficiary/charityOutOfScope, messages); both files
   JSON-parse-validated. No hard-coded strings. (en.json was concurrently modified by a
   parallel session — the insert survived, re-validated after the collision.)
10. **Component shape:** standalone OnPush, `trackByReportId`/`trackByCharityId`,
    `takeUntil(destroy$)`; no `.spec.ts` (matches the module's 6-3 re-cut shape); tests
    excluded per the standing decision.
11. **Migration NOT applied** — filed only, per the standing live-API constraint. Pending
    chain at the next restart/apply window: `Epic16_SerialUniqueBackstop` (parallel session),
    `Epic06_RetireConstructionHousing` (DROPS `HousingProject` — apply reserved for user
    confirmation), `Epic06_HousingReportBeneficiary`. Task 5's live script runs after that.
12. Backend compiled clean via the Efmig configuration build (Debug outputs are held by the
    user's running API — never killed); `npm run build` green (0 errors, pre-existing
    warnings only).

### File List

| File | Action |
| --- | --- |
| `Backend/src/IIROSA.Domain/Enums/ReportBeneficiaryType.cs` | new |
| `Backend/src/IIROSA.Domain/Entities/PeriodicOrphanReport.cs` | modified — ChildOrParent, FK_HousingFamilyId, HousingFamily nav |
| `Backend/src/IIROSA.Domain/Configurations/PeriodicOrphanReportConfiguration.cs` | modified — column, FK SET NULL, composite index |
| `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/PeriodicOrphanReportFilterDto.cs` | modified — ChildOrParent, HousingFamilyId filters |
| `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` | modified — predicates, GetHousingBeneficiaryReportsAsync, IsFamilyInCallerScopeAsync |
| `Backend/src/IIROSA.Application/Interfaces/IPeriodicOrphanReportService.cs` | modified — GetHousingBeneficiaryReportsAsync |
| `Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs` | modified — childOrParent discriminator on by-orphan |
| `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824115014_Epic06_HousingReportBeneficiary.cs` | new (+ race remarks) |
| `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824115014_Epic06_HousingReportBeneficiary.Designer.cs` | new |
| `Backend/src/IIROSA.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` | regenerated |
| `Frontend/src/app/modules/housing-projects/models/housing-project.model.ts` | modified — HousingBeneficiaryType, HousingReportListItem, HousingReportPagedResult |
| `Frontend/src/app/modules/housing-projects/services/housing-project.service.ts` | modified — getHousingBeneficiaryReports, deleteHousingReport |
| `Frontend/src/app/modules/housing-projects/housing-report-list/housing-report-list.component.ts` | new |
| `Frontend/src/app/modules/housing-projects/housing-report-list/housing-report-list.component.html` | new |
| `Frontend/src/app/modules/housing-projects/housing-report-list/housing-report-list.component.scss` | new |
| `Frontend/src/app/modules/housing-projects/housing-projects-routing.module.ts` | modified — `:id/reports` before `:id` |
| `Frontend/src/app/modules/housing-projects/housing-project-detail/housing-project-detail.component.ts` | modified — التقارير الدورية action, viewReports() |
| `Frontend/src/assets/i18n/ar.json` | modified — housingProjects.reports.* |
| `Frontend/src/assets/i18n/en.json` | modified — housingProjects.reports.* |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-06 and module spec §11.S.3 / §11.U.6; planned-screen + missing-discriminator + missing-scoping findings recorded; beneficiary design binding 6-7/6-8. |
| 2026-08-24 | Implemented: discriminator enum + columns + migration (regenerated after the stale-assembly race), scoped housing-beneficiary read (new service method, filter predicates), controller discriminator param, §11.S.3 screen with live delete and disabled 6-8/epic-18 commands, route + detail entry point, ar/en i18n. Builds green; live verification deferred to the migration-apply/restart window. |
| 2026-08-24 | Review closed: 4 patches applied (soft delete + read filters with verified slot semantics, discriminator-scoped history, PermissionGuard wiring); migration applied. Live battery 35/35. Status → done. |
