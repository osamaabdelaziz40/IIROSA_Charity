# Story 9-3: Create a periodic report

| Field | Value |
| --- | --- |
| Story key | `9-3-create-a-periodic-report` |
| Epic | EP-09 — Orphan Periodic Reports (التقارير الدورية للأيتام) |
| Use case | UC-ORR-03 — إضافة تقرير دوري |
| Priority / size | Must · 10 points |
| Specification | `docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md` (§14.S.2 screen — 53 fields, 12 commands; §14.D/25.4 detailed scenario; §14.U.3) |
| Route | `#/periodic-orphan-reports/create` (`:id/edit` is the same component — edit semantics land in 9-5) |
| Endpoint | `POST /api/PeriodicOrphanReports` |
| Depends on | **9-1** (registered module, scope, permissions) · **9-2** (orphan-by-code prefill of the header) |
| Roles | Charity → `Charity`, `SuperAdmin`, `Admin` (existing `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` on POST — keep) |

## Status

review

## Story

As a charity user, I want to be able to create a periodic report إضافة تقرير دوري, so that the
register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user on `#/periodic-orphan-reports/create`, when the actor presses «حفظ» with
   valid input, then a new record exists, owned by the charity of the orphan being reported on, and
   appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by `POST
   /api/PeriodicOrphanReports` with a typed request DTO (never untyped) and the response is rendered
   without a page reload.
3. Given a mandatory field listed in §14.S.2 is empty, when the actor saves, then the save is
   refused and the offending field is flagged.
4. Given the orphan already has a report for the same month/year, when the actor saves, then the
   save is refused with a clear duplicate message and nothing is written (unique index
   `(OrphanId, ReportMonth, ReportYear)` — verified in the entity configuration).
5. Given the save succeeds, when the actor returns to `#/periodic-orphan-reports`, then the record
   appears there with the values just entered, in state Submitted (neither accepted nor refused,
   awaiting HQ review).
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the fields of §14.S.2 are implemented with their mandatory flags and
lookups; §14.D/25.4 passes end to end; validation runs in the service layer (FluentValidation);
only the UnitOfWork saves.

**Spec note («* سبب الرفض Mandatory»):** that flag is a legacy artifact of the review screen — on
**create** neither approval flag is set (the report enters Pending), so the refusal-reason field is
not shown/required here. It becomes mandatory only in 9-8's refusal flow. The genuinely mandatory
create inputs are: the orphan (resolved via 9-2's code lookup), تاريخ التقرير, and صوره اليتيم.

**Deferred rule (recorded):** §14.D pre-condition "the charity's add permission is enabled" —
`IsPeriodicReportsEnabledForCharityAsync` is a copied `return true` stub (no Settings wiring).
Leave the stub call in place with `// TODO EP-19: charity feature flags`; do not build settings
here (17-2 deferred-rule precedent).

## Screen contract (§14.S.2 — 53 fields; the create-relevant mapping)

The entity/DTO already covers the dimensions. Group the form into the spec's sections; Arabic
labels right-to-left; every field bound to a `CreatePeriodicOrphanReportDto` key:

| Section (spec) | Fields → DTO keys |
| --- | --- |
| Header (read-only, from 9-2) | تاريخ التقرير `ReportDate` (editable date) · رقم التقرير (server-generated `ReportNo` `POR-YYYY-NNNN`, read-only) · الجمعية (read-only) · عمر اليتيم / كود اليتيم (read-only) |
| الحالة الصحية | الحالة الصحية `MedicalStatus` (سليم/معاق/مريض) · نوع المرض `Disease` + `DiseaseDescription` · نوع الاعاقة `Disability` (حركية/بصرية/سمعية/ذهنية) · تفاصيل الاعاقة `DisabilityDescription` |
| الحالة التعليمية | نوعية العمل `ProfessionName` · المرحلة الدراسية `EducationalLevelId` (lookup EducationalLevels) · الصف الدراسي `EducationalStageId` (lookup) · اسم المؤسسة `School` · نوع التعليم `SchoolType` (حكومي/أهلي) · الكلية `Faculty` · التخصص `Department`/`Specialization` · اخر تقدير `EducationDegree` (ممتاز…ضعيف) · السنة الدراسية `EducationalYear` · الحالة `DropOut`/`IsOrphanStudent` · اعلى مؤهل `HighestEducationalLevel` + `HighestEducationalLevelYear` + `EdDate`-like fields → `Grade`-adjacent entity fields · ترك الدراسة `DropOutYear`/`DropOutStageId` |
| الملفات المطلوبه | صوره اليتيم `OrphanImageId` (**mandatory**) · القيد/شهاده الطالب `OrphanCertificateImageId` · صوره التقرير الطبي `MedicalReportImageId` · شهاده وفاه `OrphanDeadImageId` · عقد زواج `OrphanMarriageImageId` — all image/* uploads stored via the Framework attachment service, referenced by id (BR-12: by reference, never embedded) |
| الانشطة والبرامج | الهوايات `Hobby` · اسم الدوره `Course`/`CourseName` · اسم الرياضه `SportName` · اسم المهنه `ProfessionName` · رساله اليتيم `OrphanMessage` · تزوج `Married` + `MarriageDate` · توفى `Dead` + `DeathDate` · طلب كفالة طالب `IsOrphanStudent` + `AnnualFeeForStudy`/`StudyingYears`/`RestStudyingYears`/`GraduationYear` |
| Religious/behaviour (entity extras the legacy form captured) | `PrayerStatus` · `MannersStatus` · `HadeethStatus` · `QuranParts` · `QuranVerses` |

Checkbox interlocks (legacy `MarriedFun/DieFun/IsOrphanStudentFun`): ticking تزوج/توفى enables and
requires its date field; unticking clears it. `IsOrphanStudent` reveals the four student-funding
numeric fields.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Service write path | `Backend/src/IIROSA.Application/Services/PeriodicOrphanReportService.cs` | `CreateReportAsync` exists: validates orphan exists, feature-flag stub, generates `ReportNo` (`POR-YYYY-NNNN`), defaults `Reviewed=false/Locked=false/Active=true`, saves via `_unitOfWork.SaveChangesAsync()` (UoW discipline already correct) |
| DTO | `DTOs/PeriodicOrphanReport/CreatePeriodicOrphanReportDto.cs` | Exists — data-annotation `[Required]` on `OrphanId`/`ReportDate` only; **no FluentValidation validator anywhere in the module** |
| API | `PeriodicOrphanReportsController.cs` `POST /` | Exists, returns 201 + DTO; no ValidationException catch (the 13-3/17-2 errors-map shape is missing) |
| Entity gaps | `PeriodicOrphanReport.cs` + configuration | Config marks `ReportYear`, `ReportMonth`, `ReviewStatus` **required** — the DTO does not carry them; the service must stamp them from `ReportDate` or every insert fails. Unique index `(OrphanId, ReportMonth, ReportYear)` will throw a raw 500 on duplicates unless handled |
| Frontend | `periodic-report-form.component.*` | Exists (create+edit modes) with the section skeleton; unregistered until 9-1; no attachment wiring, no i18n, no OnPush |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Required-column stamping missing.** `ReportYear`/`ReportMonth`/`ReviewStatus` are
   config-required but never set on create → `INSERT` fails. Stamp from `ReportDate`
   (`ReportYear = ReportDate.Year`, `ReportMonth = ReportDate.Month`, `ReviewStatus = "Pending"`).
2. **Duplicate-period 500.** The unique index violation surfaces as an unhandled `DbUpdateException`
   → 500. Catch it (or pre-check with a friendly race-aware message) and return 400 with the
   duplicate-report message (AC 4).
3. **Charity ownership not stamped.** `CharityId` must be set from the orphan's `FK_CharityId`
   server-side (BR-10), never from the payload.
4. **No validator.** Add FluentValidation; wire the controller's ValidationException catch to the
   `OfficeProjectManagementController.cs:89-101` errors-map shape.
5. **Mandatory orphan image unenforced** server-side (`OrphanImageId` nullable on the DTO with no
   rule) — enforce in the validator for the create path.
6. **No attachment upload wiring** — the five `*ImageId` fields cannot be populated from the UI
   today.

## Tasks / Subtasks

- [x] **Task 1 — Validator** (AC 3)
  - [x] `Application/Validators/PeriodicOrphanReport/CreatePeriodicOrphanReportValidator.cs` (new
        folder), modelled on `CreateOfficeProjectValidator`: `NotEmpty` for `OrphanId`,
        `ReportDate`, `OrphanImageId`; `ReportDate` not in the future; numeric ranges
        (`AnnualFeeForStudy ≥ 0`, `StudyingYears`/`RestStudyingYears`/`GraduationYear` sane
        bounds); string max lengths matching the entity configuration; conditional requires:
        `Married == true → MarriageDate` required, `Dead == true → DeathDate` required,
        `IsOrphanStudent == true → AnnualFeeForStudy` required
        *(validator skeleton existed; this story added `OrphanImageId` mandatory, not-in-future
        date, lengths PrayerStatus/MannersStatus ≤ 100 + QuranParts ≤ 50 — the only DTO-fed
        columns the configuration bounds; the rest are nvarchar(max) by convention — all three
        conditional requires, and the numeric ranges 0-30 / 1900-2100)*
- [x] **Task 2 — Service hardening** (AC 1, 4, 5)
  - [x] `ValidateAndThrowAsync` first line of `CreateReportAsync`; stamp `ReportYear`/`ReportMonth`
        from `ReportDate`, `ReviewStatus = "Pending"`, `CharityId` from the orphan's
        `FK_CharityId` (fetch the orphan once — it is already fetched for existence validation)
        *(all already in the copied/hardened `CreateReportAsync` — verified this pass)*
  - [x] Charity-bound callers may only report on their own charity's orphans (9-1 scope applied to
        the orphan gate, not just the report query) *(404-not-leak via
        `IsOrphanInCallerScopeAsync` — cross-charity orphanId reads as not-found)*
  - [x] Handle the duplicate-period case: catch `DbUpdateException` around save OR pre-query the
        index → 400 `message` = the duplicate key (i18n'd on the client)
        *(pre-query `EnsureNoDuplicateAsync` throws `BusinessException` → controller 400
        `{message}`; `IgnoreQueryFilters` so soft-deleted rows still occupy the period slot)*
  - [x] Keep `ReportNo` generation, defaults, and the feature-flag stub call (TODO marker)
        *(`POR-YYYY-NNNN` sequence per year; stub carries `TODO (epic 19)` marker)*
- [x] **Task 3 — Controller error shape** (AC 3, 6)
  - [x] Catch `FluentValidation.ValidationException` FIRST → `BadRequest(new { message, errors =
        ex.Errors.GroupBy(e => e.PropertyName ?? "").ToDictionary(…) })` — the exact
        `OfficeProjectManagementController.cs:89-101` shape (must precede the catch-all);
        duplicate/ownership failures → `BadRequest(new { message })`; keep the existing
        `[Authorize(Roles = "SuperAdmin,Admin,Charity")]` *(present verbatim on POST)*
- [x] **Task 4 — Form screen** (AC 1, 3, 5)
  - [x] Complete `periodic-report-form.component` (4-file shape, `OnPush`) per the screen-contract
        table: header from 9-2's lookup; sections الحالة الصحية / الحالة التعليمية / الملفات
        المطلوبه / الانشطة والبرامج (+ religious/behaviour fields); checkbox interlocks; `*`
        markers on mandatory fields; routes `create` + `:id/edit` already point here (add mode is
        this story's concern) *(OnPush added; interlocks wired — untick married/dead clears the
        date, untick isOrphanStudent clears the four funding fields and hides the block; the
        funding block was missing from the copied template entirely — added)*
  - [x] Attachment slots: shared `app-attachment` component (`maxFiles: 1`, `accept: "image/*"` per
        the spec's png/jpg/jpeg), upload via the Framework attachment service, keep the returned
        id in the matching `*ImageId` form control; remove commands clear the reference (BR-12)
        *(all five §14.S.2 slots driven by one `imageSlots` config loop; upload → server id →
        control patch; remove → null; صوره اليتيم marked `*`)*
  - [x] Lookups from REAL endpoints only (`nameAr ?? nameEn`, option value `id`):
        EducationalLevels etc. — see Dev Notes for what exists vs. is hardcoded tri/tetra-state;
        no hardcoded arrays where a lookup endpoint exists
        *(المرحلة الدراسية = live `GET /api/LookupManagement/education-levels` select — was absent
        from the copied template; الصف الدراسي stays free-text `Grade` — recorded below)*
  - [x] Submit → `POST`; on success toast + navigate to `#/periodic-orphan-reports` (spec
        post-condition); on failure map `error.error.errors` (PascalCase keys) onto the controls
        and flag them (15-6 field→message mapping is the reference)
        *(success toast + `../` navigate; `handleSaveError` camelCases PascalCase keys, sets
        control errors, marks touched, renders a summary alert + toast)*
- [x] **Task 5 — i18n** — every section/label/validation message under `periodicReports.form.*` in
      **both** `ar.json` and `en.json` *(50 keys per language incl. 8 section titles, 5 attachment
      slot labels, upload/save messages)*
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] `dotnet build` green (MSB3021/3027 caveat — never kill the live API) — **0 errors
        2026-08-24 (full solution)**
  - [ ] Live check (15-6 style): valid POST → 201 with `reportNo` populated and `reviewStatus:
        "Pending"`; missing orphan image → 400 with `errors.orphanImageId`; duplicate month → 400
        duplicate message; charity user posting another charity's `orphanId` → 400; unauthenticated
        → 401. Send Arabic payloads as UTF-8 from a file (inline curl shows `?????` from the
        Git-Bash codepage — artifact, not defect)
        — **PENDING the user restarting their own IIROSA.Api (pre-story binaries; never killed by
        policy — epic-4/5/8 precedent)**
  - [ ] Created row appears in the list with the entered values — browser pass pending the same
        restart
  - [x] `npm run build` green (ng-serve stale-bundle caveat) — **0 errors 2026-08-24**
  - [x] Tests: excluded per the standing user decision

## Dev Notes

### Platform rules that bind this story

- Only `IUnitOfWork` saves — the copied service already complies; keep it that way.
- FluentValidation in `Application/Validators/…`, invoked in the **service**; DB-dependent checks
  (orphan exists, duplicate period, ownership) stay in the service body.
- camelCase wire; typed DTOs only (untyped `JObject` binding is the prd.md §7 legacy defect).
- Raw envelope + `{ message, errors }` on failure — no `ApiResponse<T>` (15-1 ruling).
- Audit fields from the interceptor — never hand-stamp `CreatedOn/CreatedBy`.
- **Lookup inventory (verified):** `EducationLevel`, `HealthStatus` lookups exist in
  `IIROSA.Domain/Entities/Lookups/`; `EducationalStage`, `Professions`, `MessageReasons`,
  `EducationalLevelOfGraduate`, `EducationalLevelsLeavingStudying` do **not** exist as entities.
  The spec's own §14.S.2 renders المرحلة الدراسية/الصف as lookups and the tri-state lists
  (health, disability, school type, grade, hobbies) as inline option sets. Decision: bind
  المرحلة الدراسية/الصف الدراسي to the existing `EducationLevel` lookup family only if its rows
  carry both levels and stages (inspect before inventing); otherwise ship the tri/tetra-state
  fields as translated option constants (they are closed enums in the WAR, not user-maintained
  lists) and record the choice in the completion notes. Do NOT create new lookup tables in this
  story — that is a correct-course decision if the closed sets prove insufficient.
- `RefuseReasonId`/`MessageId` FK columns exist on the entity but their lookups don't — leave both
  untouched here (refusal reason lands in 9-8; message list is an optional legacy extra).

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Edit-mode population, «You can not update old report», resubmission clearing | 9-5 |
| Detail/read-only rendering incl. attachment galleries | 9-4, 9-16 |
| Review controls (`IsAccepted`/`IsRefused` checkboxes in §14.S.2 are review-screen concerns rendered read-only or hidden here) | 9-7, 9-8 |
| Print command on the form (`GotoPrintActionV2`) | 9-17 |
| `ShowOrphanImages`/`ShowCertificateImages` viewers (upload + reference only here) | 9-16 |

### References

- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#14.S.2] the 53-field screen contract
- [Source: docs/Modules/14-UC-ORR-Orphan-Periodic-Reports.md#25.4] detailed scenario (main flow,
  alternates A1–A3, BR-10/11/12)
- [Source: _bmad-output/planning-artifacts/epics.md#3.9] US-ORR-03 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Configurations/PeriodicOrphanReportConfiguration.cs] required
  columns + unique index `(OrphanId, ReportMonth, ReportYear)` behind AC 4
- [Source: Backend/src/IIROSA.Api/Controllers/OfficeProjectManagementController.cs:89-101]
  ValidationException → errors-map pattern to copy verbatim
- [Source: _bmad-output/implementation-artifacts/epic-15-missions/15-6-create-a-mission.md] latest reviewed create
  story: errors-map wiring, UTF-8 live-check caveat

## Dev Agent Record

### Agent Model Used

claude-sonnet-4.5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- `dotnet build Backend/IIROSA.sln` → **0 errors** (2026-08-24, full solution).
- `cd Frontend && npm run build` → **0 errors** (2026-08-24, hash d4c873ee, 44 s).
- Live battery (201/reportNo/Pending · missing-image 400 errors-map · duplicate-month 400 ·
  cross-charity orphanId 404 · unauthenticated 401) — **pending the user restarting their own
  IIROSA.Api** (port 60960 serves pre-story binaries; policy: never kill their process).

### Completion Notes List

- The service write path was already hardened beyond the story audit's snapshot —
  `CreateReportAsync` carries validator-first, orphan scope gate (404-not-leak), period stamping,
  `CharityId = orphan.FK_CharityId`, duplicate pre-check, `POR-YYYY-NNNN`, UoW-only save. This
  story's backend delta was the validator ruleset (mandatory image, future-date, lengths,
  interlock requires, numeric ranges).
- **Duplicate check uses `IgnoreQueryFilters`** — a soft-deleted report still occupies the
  orphan's `(OrphanId, ReportMonth, ReportYear)` slot, matching the DB unique index (which is
  blind to `IsDeleted`). Deliberate; prevents a resurrect-through-delete cycle.
- **Attachment wiring choice (recorded):** `app-attachment-input` (used by family-form) converts
  files to base64 and emits a client-TEMP id — it never uploads, so its ids cannot satisfy the
  `*ImageId` server-reference contract. Used the simpler `app-attachment` CVA (raw `File[]`) +
  `AttachmentService.upload(file, 'PeriodicOrphanReport')` → real server id → control patch
  (BR-12). Remove clears the control to null.
- **Lookup ruling applied:** المرحلة الدراسية binds the live `education-levels` lookup (was
  entirely absent from the copied template); الصف الدراسي stays free-text `Grade` — the spec's
  stage lookup family has no Domain entity (Dev Notes), and `EducationLevel` rows do not carry a
  level/stage split. Closed tri/tetra-state sets (health, disability, school type, education
  degree) remain free-text inputs pending a correct-course decision — NOT invented as lookup
  tables here.
- **Student-funding block was missing** from the copied template (the four `IsOrphanStudent`
  fields existed only in the form model). Added, revealed by the interlock, cleared on untick.
- Server-error mapping camelCases the PascalCase FluentValidation keys
  (`OrphanImageId` → `orphanImageId`) before writing control errors — 15-6 shape.
- i18n: 50 `periodicReports.form.*` keys per language. Detail/review-screen keys
  (`orphanInformation`, `religiousBehavioral`, …) belong to 9-4/9-7 and land there.

### File List

**Backend**

- `Backend/src/IIROSA.Application/Validators/PeriodicOrphanReport/CreatePeriodicOrphanReportValidator.cs` —
  full ruleset (mandatory OrphanId/ReportDate/OrphanImageId, not-in-future, lengths, three
  conditional requires, numeric ranges)

**Frontend**

- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-form/periodic-report-form.component.ts` —
  OnPush; AttachmentService + LookupManagementService injected; `imageSlots` config + upload
  handlers; `wireInterlocks()` (married/dead/isOrphanStudent); `handleSaveError` errors-map;
  success toasts; trackBy helpers; `serverErrorsKeys`
- `Frontend/src/app/modules/periodic-orphan-reports/periodic-report-form/periodic-report-form.component.html` —
  educationalLevel select (live lookup); student-funding block; الملفات المطلوبه section with
  five `app-attachment` slots + `*` on صوره اليتيم; server-error summary alert
- `Frontend/src/assets/i18n/ar.json` + `en.json` — `periodicReports.form.*` 50 keys each

**Board**

- `_bmad-output/implementation-artifacts/sprint-status.yaml` — `9-3-create-a-periodic-report`:
  `ready-for-dev → in-progress → review`

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-ORR-03 and module spec §14.S.2 / §14.D-25.4; required-column stamping and duplicate-index defects recorded; refusal-reason mandatory flag ruled a review-flow artifact. |
| 2026-08-24 | Implemented: validator ruleset completed, form screen finished (live educational-level lookup, five BR-12 attachment slots with real server ids, interlocks, student-funding block, errors-map + toasts, OnPush), 50 form i18n keys ar/en. Backend service/controller verified already-hardened. Builds 0 errors. Live checks pending API restart. Status → review. |


### Review Findings (epic review 2026-08-24)

> Review Outcome 2026-08-24 — all items patched & verified: P7 delivered as the [IsDeleted]=0 addition to the child-slot filtered unique index (config + migration 20260824203000 + service guard comment) — supersedes epic-6's “soft-deleted rows hold slots” note. P8 verified non-defect: migration 20260824115014 already ships the filtered-unique family+month index; story claim corrected to TRUE.

- [x] [Review][Patch] P4 Null-CharityId stamp at create — report invisible to own charity + 201 null body [PeriodicOrphanReportService.cs create]
- [x] [Review][Patch] P7 Soft-delete squats (orphan,year,month) unique slot — filtered unique index (WHERE IsDeleted=0) [InitialCreate IX + create guard]
- [x] [Review][Patch] P8 Guardian one-per-family-month: plain IX, not filtered-unique (story claim false) [migration 20260824115014]
- [x] [Review][Patch] P9 Duplicate race — raw 500 not friendly 400 [create path]
- [x] [Review][Patch] P14 Coded-orphan pre-condition never enforced server-side on create
- [x] [Review][Patch] P47b Form default reportDate = UTC yesterday for Riyadh 00:00-03:00 [periodic-report-form.component.ts:200]
