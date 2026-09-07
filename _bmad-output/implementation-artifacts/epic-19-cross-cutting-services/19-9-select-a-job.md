# Story 19-9: Select a job

| Field | Value |
| --- | --- |
| Story key | `19-9-select-a-job` |
| Epic | EP-19 — Cross-Cutting Services — الخدمات المشتركة |
| Use case | UC-SYS-09 — المهنة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/24-UC-SYS-Cross-Cutting-Services.md` (§24.U.9 scenario) |
| Route | none yet — the catalogue feeds the guardian (provider/parent) job fields on the family forms; admin CRUD lands on the lookup-management family |
| Endpoint | `GET /api/LookupManagement/jobs` (new — endpoint ruling below) |
| Depends on | 19-4 pattern (dedicated-endpoint ruling + `LookupServiceBase` chain); 19-5 sibling (same shape, MaritalStatus); consumers are EP-05 family forms |
| Roles | All roles read (authenticated); catalogue writes HQ-admin |

## Status

done

## Story

As a signed-in user, I want to be able to select a job المهنة for a guardian or provider, so
that the family's income picture is complete for means assessment.

## Acceptance Criteria

1. Given a signed-in user with an active session, when a family/guardian form loads its job
   field, then the job catalogue is served — no stored data is changed.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/jobs` and the response is rendered on the screen without a page
   reload.
3. Given the job field is requested, when the read is served, then the list comes from a
   dedicated job catalogue (`LookupEntity`, `NameAr`/`NameEn`, `IsActive` respected) — the
   same shape as every other 19-x catalogue.
4. Given the catalogue is empty, then the consuming control renders its empty option — not an
   error (8-11 AC 6 precedent) — and the shipped seed means fresh databases are not empty.
5. Given the session has expired or the role is not permitted, when the function is invoked,
   then the request is rejected and the actor is routed back to the login screen.

> **Endpoint ruling (recorded deviation):** the board's traceability names
> `GET /api/Reports` for US-SYS-09 — a legacy WAR artifact, not a real contract (ReportsController
> serves the 41 report sheets, EP-18). The correct platform realisation is a **dedicated
> `jobs` catalogue on `LookupManagementController`** per the 19-4 dedicated-endpoint ruling:
> `GET /api/LookupManagement/jobs`. Nothing is added to ReportsController.

**Definition of done:** the jobs catalogue exists with the full 7-file chain + seed;
`lookup-management.service.getJobs()` exists; the family/guardian form wiring deferral to
EP-05 is recorded (8-11 precedent); the endpoint ruling is in the story.

## What exists already (copied from the previous implementation — DO NOT rebuild)

| Layer | File | State |
| --- | --- |
| Pattern | `LookupServiceBase` 7-file chain — shipped twice by 8-11 (education-levels, health-statuses), sibling 19-5 (marital-statuses) | Copy verbatim |
| Controller family | `LookupManagementController` — the catalogue home; refugee-form set precedent for where the action goes | Live |
| Frontend idiom | `lookup-management.service.ts` method-per-catalogue; `app-drop-down` + `LookupService` cache for dropdowns | Live |
| **Job catalogue** | **No `Job` entity, no jobs endpoint exists** | This story builds it |

## Verified gaps this story must fix

1. **No Job catalogue.** Build the full stack: entity + configuration, repo interface + typed
   repo, service interface + `LookupServiceBase` service, DTO trio, AutoMapper profile, DI
   registration, controller action (`GET jobs`, read-first; the CRUD quad can follow the
   banks/departments shape only if the admin screen in this story needs it — see Task 3).
2. **Seeding.** Ship an existence-guarded, Arabic-first starter set of professions
   (e.g. موظف حكومي، موظف قطاع خاص، عامل، تاجر، مزارع، ربة منزل، متقاعد، بدون عمل — with
   `NameEn` mirrors) so fresh databases render a usable dropdown.

## Tasks / Subtasks

- [x] **Task 1 — Job lookup stack** (AC 2, 3)
  - [x] `Job : LookupEntity` (`MappingDefaults.LOOKUP_SCHEMA`, no manual `DbSet`) +
        configuration; `IJobRepository` + typed repo; `IJobService` + `LookupServiceBase`
        service; DTO trio; AutoMapper profile; DI; `GET jobs` action — byte-for-byte the 8-11
        shape (`{ id, nameAr, nameEn }`, `IsActive` respected, raw envelope)
  - [x] No new caching (platform-consistent)
- [x] **Task 2 — Seed** (AC 4)
  - [x] Existence-guarded Arabic-first starter professions via the established seeder idiom
        (`IsActive = true`)
- [x] **Task 3 — Frontend service + admin surface** (AC 1)
  - [x] `lookup-management.service.ts`: `getJobs()` (jobs section header, mirroring 19-5's
        fix if it has landed)
  - [x] Family/guardian form wiring belongs to EP-05 (8-11 precedent) — leave the endpoint +
        service live and record the deferral. An admin CRUD screen is **optional**; if added,
        follow the banks/departments screen shape and the 19-4 parallel-work policy audit
- [x] **Task 4 — Verification** (AC 5)
  - [x] Builds green; live smoke on the private port: `jobs` → 200 seeded rows,
        unauthenticated → 401. Tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- **`GET /api/LookupManagement/jobs` — nothing on ReportsController** (ruling above).
- Raw envelope (2026-08-19 standing decision); `LookupEntity` (int), LOOKUP_SCHEMA, no manual
  `DbSet`; `NameAr ?? NameEn` labels; catalogue reads are global reference data (16-8/19-4
  ruling — jobs are not charity-scoped; the guardian record carries the tenancy).
- **EF migration — same hazard set as 19-5:** the migration must contain ONLY the Job table.
  Check the last applied migration chain first (`dotnet ef migrations list`; parallel-session
  bundling hazard) and hand-prune any spurious `DropTable("ApplicationUser")` the stale
  snapshot emits (epic-15 recorded hazard). If 19-5's MaritalStatus migration is still
  unapplied in the same chain, coordinate — one combined "Epic19 lookups" migration is
  acceptable if both sessions agree; otherwise stay additive-only.

### Out of scope (other stories — do not build)

| Item | Story |
| --- | --- |
| Marital-status catalogue | 19-5 (sibling — coordinate the migration) |
| Guardian/family form wiring | EP-05 (recorded deferral, 8-11 precedent) |
| Generic sweep (caps, gating) | 19-4 |
| Report sheets | EP-18 |

### References

- [Source: docs/Modules/24-UC-SYS-Cross-Cutting-Services.md#24.U.9] scenario + §24.2 UC-SYS-09 row
- [Source: _bmad-output/planning-artifacts/epics.md#3.19] US-SYS-09 acceptance criteria + traceability (GET /api/Reports → ruling)
- [Source: _bmad-output/implementation-artifacts/epic-8-orphan-register-and-coding/8-11-load-orphan-reference-data.md] the 7-file chain pattern + wiring-deferral precedent
- [Source: Backend/src/IIROSA.Application/Services/LookupManagementService.cs] `LookupServiceBase` — the base to derive from

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- EF route (recorded 19-5 workaround, reused): build Api -c Release -> ef migrations add --configuration Release --no-build -o Data/Migrations --context ApplicationDbContext (the bare command fails: two DbContexts) -> migration verified isolated -> rebuild Release -> database update --configuration Release --no-build. Applied 20260825101448_Epic19_JobLookup cleanly.
- Seeder-gate discovery: InitializeAsync runs only when ASPNETCORE_ENVIRONMENT=Development or first run (no identity users) — Program.cs:637. The Production-mode smoke restart correctly skipped seeding (jobs -> []); Development restart seeded 8 rows. Platform behavior, not a defect.
- Live battery (61970, Development restart): GET /api/LookupManagement/jobs -> 200, 8 rows (موظف حكومي ... بدون عمل, all isActive=true, NameEn mirrors); Charity role -> 200; unauthenticated -> 401.
- Builds: Release 0 errors; bin/Smoke 0 errors; tsc --noEmit clean outside pre-existing spec-typing noise.
### Completion Notes List

- **Task 1 (7-file chain, byte-for-byte the MaritalStatus shape):** Job.cs + JobConfiguration (Lookup schema, no manual DbSet); IJobRepository + JobRepository; IJobService + JobService : LookupServiceBase; JobDto/CreateJobDto/UpdateJobDto trio; LookupProfile 3 maps; DI (service + repo lines); controller GET jobs action (DropdownPageSize, IsActive-respected, raw envelope, class-level JWT). No caching added (platform-consistent). Board's GET /api/Reports traceability NOT implemented — endpoint ruling held (nothing touched ReportsController).
- **Task 2 (seed):** SeedJobsAsync joined GuardianLookupSeedData.SeedAllAsync (the guardian catalogue seeder, Step 9 — no new initializer step): 8 Arabic-first professions with NameEn mirrors from the story's list, existence-guarded.
- **Task 3 (frontend):** getJobs() added under the GUARDIAN REFERENCE DATA section, mirroring getMaritalStatuses. Admin CRUD screen deliberately NOT added (story marks it optional; no admin surface asked for a jobs tab — the catalogue is read-side today). Family/guardian form wiring deferred to EP-05 (8-11 precedent) — Father/Mother/Provider/Relative carry free-text string Job columns today; converting them to a JobId FK is EP-05 territory.
- Migration isolation verified: single CreateTable Lookup.Job + symmetric DropTable Down — no spurious drops (the epic-15 stale-snapshot hazard did not recur; the Release route avoids the stale Debug bin).
- Browser click-through deferred to the batched live walkthrough (epic precedent).
### File List

- Backend/src/IIROSA.Domain/Entities/Lookups/Job.cs (new)
- Backend/src/IIROSA.Domain/Configurations/JobConfiguration.cs (new)
- Backend/src/IIROSA.Domain/Interfaces/ILookupRepository.cs (IJobRepository)
- Backend/src/IIROSA.Infrastructure/Data/Repository/LookupRepository.cs (JobRepository)
- Backend/src/IIROSA.Application/Interfaces/ILookupService.cs (IJobService)
- Backend/src/IIROSA.Application/Services/LookupManagementService.cs (JobService)
- Backend/src/IIROSA.Application/DTOs/LookupManagement/LookupDtos.cs (Job DTO trio)
- Backend/src/IIROSA.Application/Profiles/LookupProfile.cs (Job maps)
- Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs (Job DI x2)
- Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs (IJobService DI + GET jobs)
- Backend/src/IIROSA.Infrastructure/Data/SeedData/GuardianLookupSeedData.cs (SeedJobsAsync)
- Backend/src/IIROSA.Infrastructure/Data/Migrations/20260825101448_Epic19_JobLookup.cs (+Designer, snapshot)
- Frontend/src/app/modules/lookup-management/services/lookup-management.service.ts (getJobs)
## Change Log

| Date | Change |
| --- | --- |
| 2026-08-25 | Story file created from `epics.md` US-SYS-09 and module spec §24.U.9; Job catalogue confirmed absent — story delivers the 7-file chain + seed on `LookupManagement/jobs` (board's `GET /api/Reports` traceability ruled a legacy artifact); form wiring deferred to EP-05. |
| 2026-08-25 | Implemented + verified (review-and-complete pass): full 7-file Job chain + isolated migration applied via the Release EF route; 8-row Arabic-first seed joined the guardian seeder (seeder Development-gate discovered and recorded); getJobs() shipped; EP-05 wiring deferral recorded; live battery green (200 x8 rows, Charity 200, 401 unauth). |
