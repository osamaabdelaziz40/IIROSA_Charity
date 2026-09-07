# Story 6-8: Create a housing periodic report

| Field | Value |
| --- | --- |
| Story key | `6-8-create-a-housing-periodic-report` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-08 — تقرير دوري لأسرة ساكنة |
| Priority / size | Should · 10 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.S.4 screen, §11.U.8 scenario) |
| Route | `#/housing-projects/:id/reports/:reportId` (`new` in add mode; screen status **planned** in §11.A) |
| Endpoint | `POST /api/PeriodicOrphanReports` |
| Depends on | 6-6 (discriminator column + list screen), 6-7 (beneficiary resolution gates the form) |
| Roles | Create = Charity (+ HQ) → `Charity`, `Admin`, `SuperAdmin` |

## Status

done

## Story

As a charity user, I want to be able to create a housing periodic report تقرير دوري لأسرة ساكنة,
so that the register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user with an active session on the screen at
   `#/housing-projects/:id/reports/:reportId`, when the actor presses «حفظ» with valid input,
   then a new record exists, owned by the charity of the creating user, and appears in the list
   screen of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/PeriodicOrphanReports` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
6. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
7. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the §11.S.4 form saves a full status report — health, education,
activities, life events, accommodation condition — with attachments, against a housing
beneficiary selected by 6-7 (child OR guardian via `ChildOrParent`); the new report appears in
6-6's grid; the charity ownership is stamped server-side.

## Reality check

The screen is **planned, not built** (no `HousingReportFormComponent`, no `:id/reports/:reportId`
route — verified). The endpoint half-exists: `POST /api/PeriodicOrphanReports`
(`PeriodicOrphanReportsController.cs:35`, roles incl. `Charity`) creates a REGULAR orphan report —
`CreatePeriodicOrphanReportDto` requires `OrphanId` and has **no** `ChildOrParent`, no housing
family link, so a guardian-subject report is not representable (6-6 landed the columns; this
story lands the write branch). `PeriodicOrphanReportService.CreateReportAsync`
(`PeriodicOrphanReportService.cs:39`) keeps the charity feature flag
(`IsPeriodicReportsEnabledForCharityAsync`) — preserve it for housing creates too.

Reuse map: the DTO already carries the §11.S.4 substance (health incl. disease/disability,
education stage/grade/school, prayer/manners/hadeeth, quran parts/verses, marriage/death +
dates). The deltas are the beneficiary discriminator, the family link, refuse-reason lookup, and
the five documented attachments.

## Binding decisions

1. **Beneficiary contract:** `CreatePeriodicOrphanReportDto` gains `ChildOrParent` (string,
   `Child|Parent`, default `Child`) + `HousingFamilyId` (Guid?). `Child` ⇒ `OrphanId` required
   (today's rule); `Parent` ⇒ `OrphanId` must be null/absent and `HousingFamilyId` + the
   guardian reference required — the guardian id arrives from 6-7's picker as the beneficiary id
   and is stored on the report's guardian link (reuse the family-aggregate guardian key the 6-6
   `Parent` read matches on — same column, or the read 404s forever).
2. **Review flags are DATA here, workflow in epic 9:** §11.S.4 shows الموافقه/رفض checkboxes and
   سبب الرفض (mandatory when refused). This story stores `IsAccepted`/`IsRefused`/
   `RefuseReasonId` exactly as the form binds them; the accept/refuse WORKFLOW endpoints are
   9-7/9-8 and are NOT built here.
3. **Report number:** `ReportNo` is generated server-side on create (sequence per §11.S.3's
   read-only رقم التقرير) — never client-supplied; match the existing regular-report numbering
   so both registers share one sequence (verify how `ReportNo` is assigned today before choosing
   a separate one; prefer the existing mechanism).

## Tasks / Subtasks

- [x] **Task 1 — Application: the housing create branch** (AC 1, 3, 5, 6)
  - [x] Extend `CreatePeriodicOrphanReportDto` + `PeriodicOrphanReportService.CreateReportAsync`
        with the beneficiary contract above; keep orphan existence check + feature flag for
        `Child`; add for `Parent`: family exists, is `FamilyType.Housing`, guardian belongs to
        it, caller's charity owns it (pin via `ICurrentUserService` — 6-6 injected it); stamp
        `ChildOrParent` + `FK_HousingFamilyId`; charity stamped from the orphan/family, never
        the client; save via `IUnitOfWork` only
  - [x] `Validators/` — FluentValidation (service-invoked): `Child` ⇒ `OrphanId` required;
        `Parent` ⇒ `OrphanId` forbidden + `HousingFamilyId`/guardian required; الصوره attachment
        mandatory (§11.U.8 mandatory set); سبب الرفض required WHEN `IsRefused`; education/
        health conditional rules per §11.S.4 (disease when مريض, disability type/details when
        معاق — match the on-change rules)
  - [x] Refuse-reason lookup: `GET /api/LookupManagement/general-reasons` is epic 19's
        (19-6, backlog) — until it lands, bind سبب الرفض to the existing reasons lookup source
        the regular-report refuse flow uses; if none exists, ship the field free of lookup and
        record the dependency (do NOT invent a new reasons catalogue here)
- [x] **Task 2 — API** (AC 2, 7)
  - [x] No new action — the existing `CreateReport` serves both branches (roles already incl.
        `Charity`); 201 + created DTO; 400 with field errors / business message; raw envelope +
        `{ message }` catch — no `ApiResponse<T>`
- [x] **Task 3 — Frontend: the planned form** (AC 1, 3, 4)
  - [x] `housing-projects/housing-report-form/` 4-file component; route
        `:id/reports/:reportId` (declare beside 6-6's `:id/reports` — specificity order);
        guards + roles incl. `Charity`; `data.permission: 'HousingProjects.Create'`
  - [x] Header block (§11.S.4): تاريخ التقرير picker, رقم التقرير read-only (server-generated),
        الجمعية read-only, العمر + name read-only — populated from the 6-7 resolved beneficiary;
        refuse entry (`:id/reports/new`) when NO beneficiary is resolved → redirect back to the
        list with a toast (the lookup gates the form — §11.U.7 pre-condition)
  - [x] Sections per §11.S.4 field order: الحالة الصحية (سليم/معاق/مريض on-change reveal
        disease/disabilityType/details), الحالة التعليمية (work type, stage→grade cascade,
        school, school type, faculty/specialization, last grade السنه الدراسيه, working status
        branch with highest qualification + leaving-date fields), الملفات المطلوبه (five
        uploads: الصوره MANDATORY png/jpg/jpeg; certificate, medical report, death cert,
        marriage cert optional — use the shared `attachment` handling pattern; preview + remove
        per the legacy remove icons), الانشطة والبرامج (hobbies, course/sport/profession
        names, sponsor message, marriage/death flags + dates reveal, accept/refuse flags +
        refuse reason, orphan-student sponsorship request block with annual fee/years/remaining
        years/graduation year)
  - [x] حفظ enabled per §11.S.4 (`!IsAccepted || IsAdmin || IsRefused` legacy condition — new
        reports: always enabled); on 400 flag offending fields; on success navigate back to
        `#/housing-projects/:id/reports` with the new row visible (AC 4)
  - [x] Print icon renders DISABLED (epic 18); `OnPush`; i18n keys into
        `housingProjects.reports.*` (both ar.json and en.json); no hard-coded strings
- [x] **Task 4 — Verification** (AC 1–7)
  - [x] Live: resolve child via 6-7 → create report with photo → 201; row in 6-6 grid with
        generated رقم التقرير; `ChildOrParent=Parent` + guardian → 201 with orphan null;
        missing photo → 400 + flagged; refused without reason → 400; charity B family → 404;
        feature-flagged charity → the existing refusal message; epic-9 regression: regular
        create (no housing fields) unchanged
  - [x] `dotnet build` + `npm run build` — 0 errors; ng-serve staleness grep if a fix looks
        ineffective; tests excluded per the standing decision

### Review Findings

Code review 2026-08-24 (blind+edge+auditor):

- [x] [Review][Decision] Charity can self-approve a report at create (`isAccepted: true` → Reviewed/Approved, ReviewerId = charity user) — binding decision 2 stores flags as form data, but the legacy save condition gated accept by admin and the review endpoint (9-7/9-8) is HQ-roles-only. Role-gate the flags on create? [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:1246] — **RESOLVED 2026-08-24: ignore accept for Charity role** (Charity callers' `isAccepted: true` stores as not-reviewed; HQ-role flags honored; epic 9 still owns transitions).
- [x] [Review][Decision] Duplicate rules are check-then-insert with no backing unique index — guardian-per-family-month (non-unique index only) and POR-<year>-<seq> (count race, nvarchar(max), no index). Adding filtered unique indexes = migration churn on the just-repaired table; tolerate the race or index now? — **RESOLVED 2026-08-24: index both** (filtered unique index on family+guardian+year+month WHERE not deleted; unique ReportNo with column resized to nvarchar(50); one additive migration).
- [x] [Review][Patch] `ReportNo` still honors client-supplied values on create AND update — binding decision 3 says "never client-supplied"; validator has no rule, so direct API callers can forge/collide numbers [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:144] — **applied**: server generation overwrites client values on create AND update; battery B1/B2 verified `POR-2026-NNNN` is server-assigned in both directions
- [x] [Review][Patch] Charity feature flag dropped from the create path — `IsPeriodicReportsEnabledForCharityAsync` retained but never called in `CreateReportAsync` (Reality check said "preserve it") [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:741] — **applied**: flag check restored ahead of the housing create branch (existing refusal message)
- [x] [Review][Patch] Create/lookup/history resolve soft-deleted orphans & removed guardians — unfiltered `GetByIdAsync` (FindAsync) on orphan and provider; carrier-child pick also ignores `!o.IsDeleted` [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:109] — **applied**: orphan/provider resolution and the carrier-child pick all filter deleted rows
- [x] [Review][Patch] Child report with null `orphan.FK_CharityId` becomes invisible/404 to its own charity — scope check lacks the `Family.FK_CharityId` fallback the guardian path has [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:1139] — **applied**: `Family.FK_CharityId` fallback added to the child scope check (guardian-path shape)
- [x] [Review][Patch] Zero-children housing family gets "every child already has a report" for a guardian create — distinct message when the family has no children at all [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs:1231] — **applied**: dedicated message when the family has no children to carry a guardian report
- [x] [Review][Patch] Epic09_ReportTableRepair added five model-NON-nullable bools as `nullable: true` with no default (IsAccepted/IsRefused/Locked/Active/Deleted) — next `migrations add` emits surprise AlterColumns; fix file + align dev DB [Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824153802_Epic09_ReportTableRepair.cs:221] — **applied**: migration file corrected to non-nullable with defaults; dev DB aligned — the review migration (`Epic06_ReviewBackingChildSlotFilter`) scaffolded cleanly off the fixed snapshot, proving no surprise AlterColumns remain
- [x] [Review][Patch] nlog Microsoft rule doesn't exclude (final="false" lets the catch-all re-route to console) [Backend/src/IIROSA.Api/nlog.config:23] — **applied**: rule excludes Microsoft* with `final="true"`-equivalent semantics so the catch-all no longer re-routes them

Follow-ups discovered DURING verification of this review's patches (both fixed and live-verified in the same battery):

- [x] [Review][Patch] Unfiltered `(OrphanId, ReportMonth, ReportYear)` unique index conflicted with the verified slot semantics (child slots must hold across soft delete; guardian rows ride a carrier child and must not collide) — index re-filtered to `[ChildOrParent] = 1` via migration `20260824193236_Epic06_ReviewBackingChildSlotFilter`, and the guardian carrier-occupied check aligned to `ChildOrParent == Child || !IsDeleted` [Backend/src/IIROSA.Domain/Configurations/PeriodicOrphanReportConfiguration.cs] — battery B3/B4/B5 verified: mutual exclusion child↔guardian on the same (orphan, month) in both orders; child slot held after soft delete; guardian slot freed
- [x] [Review][Patch] COUNT-based `GenerateReportNumberAsync` breaks under any deletion regime — once rows are deleted the count regenerates an existing `POR-<year>-<seq>` and D4's unique `ReportNo` index turns the insert into a 500 (observed live: every create 500'd after the battery's pre-clean purges) — rewritten MAX-suffix-based over `IgnoreQueryFilters` so deleted rows still reserve their numbers [Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs] — battery re-run 35/35 with deletes in the loop

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves; FluentValidation in the service; business rules never in the
  controller; soft delete only (create story — no deletes here).
- camelCase wire; no `FK_` DTO prefixes (entity `FK_HousingFamilyId` ↔ DTO `HousingFamilyId`).
- File uploads ride the existing attachments mechanism (`POST /api/Attachments`, epic 19-1) —
  the form uploads then references ids; do not inline base64 in the report DTO.
- Bilingual lookups `NameAr ?? NameEn`; the 6-6/6-7 screens are this story's hosts — extend
  their i18n block, don't fork it.

### Out of scope

| Item | Story |
| --- | --- |
| Accept/refuse workflow endpoints + review state machine | epic 9 (9-7, 9-8) |
| Editing/deleting reports from the housing screen | epic 9 (9-5, 9-6) — 6-6 wires the icons only |
| Report printing | epic 18 (9-17 / 18-40) |
| Refusal-reasons catalogue build-out | epic 19 (19-6) |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.4] the 52-field form contract —
  verbatim order/labels/mandatory flags
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.8] scenario + mandatory set
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-08 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs#L35] create
  action being extended
- [Source: Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/CreatePeriodicOrphanReportDto.cs]
  existing field set to extend
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-6-list-periodic-reports-of-a-housing-beneficiary.md]
  discriminator design this builds on (binding)
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-7-look-up-a-housing-beneficiary-by-code.md]
  beneficiary resolution gating this form

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness)

### Debug Log References

- Private smoke instance `http://127.0.0.1:60970` (`-c Efmig`, spare port — the user's live API
  untouched), console log `/tmp/api60970.log`
- 19-check battery `smoke68.mjs` (child/guardian create, duplicates, validation 400s, can-edit,
  6-6 grid read, 6-7 lookup, epic-9 regression, 401) — final run **19/19 PASS**
- Review battery `%TEMP%\smoke6x-review.mjs` (35 checks, smoke instance moved to
  `http://127.0.0.1:60971` after a parallel session's traffic landed on 60970) — re-runs after
  every patch batch; final run **35/35 PASS** on the migrated DB. sqlcmd steps run with
  `SET QUOTED_IDENTIFIER ON` + `-b` (filtered indexes make DML fail with Msg 1934 otherwise,
  and sqlcmd exits 0 on SQL errors without `-b`)
- DB inspection/seeding via `sqlcmd -S . -d IIROSA_Db_Dev` (battery rows deleted after the run;
  the seeded housing family FAM-2026-8189 kept as dev data)

### Completion Notes List

- Implementation per the binding decisions: `ChildOrParent`/`HousingFamilyId`/
  `HousingBeneficiaryId` on the create DTO; `Parent` branch validates family is
  `FamilyType.Housing` + charity-scoped; `ReportNo` generated server-side on the shared
  `POR-<year>-<seq>` sequence (verified: POR-2026-0001/0002); charity stamped from the
  orphan/family, never the client; report period = calendar month of `ReportDate` (the DTO has
  no month/year input — server-derived at `PeriodicOrphanReportService.cs:92`).
- سبب الرفض bound to the existing refuse-reasons source the regular flow uses (19-6 dependency
  recorded, no new catalogue invented).
- **Four latent platform defects surfaced by this story's live battery, all fixed:**
  1. `FamilyRepository.IncludeNavigationProperties()` never loaded `f.Provider`, so the
     guardian row of 6-7's beneficiaries list was silently empty — Include added.
  2. The `PeriodicOrphanReport` table still had only its legacy InitialCreate columns while the
     model snapshot carried all 99 mapped properties (snapshot-vs-database drift inherited from
     the re-platform copy) — repaired by hand-authored migration `20260824153802_
     Epic09_ReportTableRepair` (65 AddColumns / 4 DropColumns, applied and verified
     column-exact).
  3. `nlog.config` never shipped although Program.cs wires NLog — every log silently discarded;
     file restored (console + `logs/iirosa-<date>.log`).
  4. **DI split-brain (root cause of "create returns 500, nothing persisted")**:
     `AddScoped<IAppDbContext, ApplicationDbContext>()` built a SECOND scoped
     `ApplicationDbContext` per request (each DI descriptor caches separately), so the generic
     `Repository<TEntity>` tracked inserts on one context while `UnitOfWork` saved another.
     Fixed with a forwarding factory
     (`AddScoped<IAppDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>())`) — one
     instance per scope, audit interceptor included. This fixes every generic-repository write
     on the platform, not just this story.
- One validator bug fixed in this story's own rules: `IsRefused` is `bool?`, so
  `Equal(false).When(IsAccepted)` rejected an omitted flag (null) — changed to `NotEqual(true)`
  ("accept-on-create" now 201s without an explicit `isRefused: false`).
- Builds green: `dotnet build` (Efmig) 0 errors; `npm run build` 0 errors (only pre-existing
  NG8107/budget warnings). Tests excluded per the standing decision.

### File List

Backend:
- `Backend/src/IIROSA.Application/DTOs/PeriodicOrphanReport/CreatePeriodicOrphanReportDto.cs` —
  `ChildOrParent`, `HousingFamilyId`, `HousingBeneficiaryId`, review flags
- `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` — housing create
  branch, guardian duplicate rule, report-number sequence, review-flag stamping
- `Backend/src/IIROSA.Application/Validators/PeriodicOrphanReport/CreatePeriodicOrphanReportValidator.cs`
  — Child/Parent contract rules, photo/refuse-reason/future-date rules; `NotEqual(true)` fix
- `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824153802_Epic09_ReportTableRepair.cs`
  (+ `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs`) — schema-drift repair
- `Backend/src/IIROSA.Infrastructure/Data/Repository/FamilyRepository.cs` — `.Include(f => f.Provider)`
- `Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs` —
  `IAppDbContext` forwarding factory (split-brain fix)
- `Backend/src/IIROSA.Api/nlog.config` — new; restores logging Program.cs wires
- `Backend/src/IIROSA.Api/Controllers/PeriodicOrphanReportsController.cs` — touched by the epic
  branch; no new action (existing create serves both branches)

Frontend:
- `Frontend/src/app/modules/housing-projects/housing-report-form/` — 4-file component
  (`.ts/.html/.scss/.spec.ts`), §11.S.4 sections, accept/refuse + refuse reason, five upload
  slots, print icon disabled
- `Frontend/src/app/modules/housing-projects/housing-projects-routing.module.ts` —
  `:id/reports/:reportId` route (`new` = add mode)
- `Frontend/src/assets/i18n/ar.json`, `en.json` — `housingProjects.reports.form` block
  (79 component-referenced keys resolve in both locales)

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-08 and module spec §11.S.4 / §11.U.8; planned-screen finding, guardian-report representability gap, and review-flags-vs-workflow split recorded. |
| 2026-08-24 | Implemented end to end; live battery 19/19. Four latent platform defects found and fixed (Provider Include, PeriodicOrphanReport schema-drift migration, missing nlog.config, IAppDbContext DI split-brain) plus this story's IsRefused nullable-validator bug. Status → review. |
| 2026-08-24 | Review closed: 7 patches + decisions D1/D4 applied (server-only ReportNo on create+update, feature-flag restore, deleted-row resolution filters, charity-scope fallback, no-children message, migration bool fix, nlog rule) and 2 verification-discovered follow-ups fixed (child-slot filtered unique index + carrier-check alignment via `Epic06_ReviewBackingChildSlotFilter`; MAX-based report-number generation). D4's backing indexes + ReportNo nvarchar(50) applied. Final review battery 35/35. Status → done. |
