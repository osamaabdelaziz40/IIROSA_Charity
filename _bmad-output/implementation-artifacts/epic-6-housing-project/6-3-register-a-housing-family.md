# Story 6-3: Register a housing family

| Field | Value |
| --- | --- |
| Story key | `6-3-register-a-housing-family` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-03 — اضافة أسرة ساكنة |
| Priority / size | Should · 10 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.S.2 screen, §11.U.3 scenario) |
| Route | `#/housing-projects/:id/edit` (add mode; spec also names `#/housing-projects/create` — one component, both routes) |
| Endpoint | `POST /api/HousingProjects/projects` |
| Depends on | 6-1 (discriminator), 6-5 (buildings/flats lookups + `Family` allocation columns — land 6-5 FIRST, recommended order) |
| Roles | Create = Charity (+ HQ) → `Charity`, `Admin`, `SuperAdmin` |

## Status

done

## Story

As a charity user, I want to be able to register a housing family اضافة أسرة ساكنة, so that the
register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user with an active session on the screen at `#/housing-projects/:id/edit`,
   when the actor presses «حفظ» with valid input, then a new record exists, owned by the charity
   of the creating user, and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by
   `POST /api/HousingProjects/projects` and the response is rendered on the screen without a page
   reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor
   saves, then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given the business rule behind «أحد المعيلين مكرر من قبل أكثر من مرة» is broken, when the
   operation is attempted, then it is refused with that message and nothing is written.

**Definition of done:** the screen fields of §11.S.2 are implemented with their mandatory flags and
lookups; the scenario of §11.U.3 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Reality check: this story re-cuts the invented construction controller — the heaviest story of the epic

Board `done` was endpoint-NAME matching: `POST /api/HousingProjects/projects` exists
(`HousingProjectsController.cs:137`) but it creates a **construction project**
(`CreateHousingProjectDto`: budget, donor, contractor — chapter 11 has no such concept). The
Angular form (`housing-project-form.component.ts:140-186`) collects construction fields, not the
§11.S.2 family file. Full audit in 6-1's "Reality check" — ruling: re-cut both sides, drop the
invented capability (epic-11 precedent).

What exists and MUST be reused:

| Exists | Where | Use |
| --- | --- | --- |
| Family create path | `POST /api/Families` → `FamilyService` (epic 5, done) + `modules/families/family-form` with father/mother/provider/orphan subforms | the housing create MIRRORS this machinery — do not rebuild family capture from scratch |
| Allocation lookups + columns | 6-5 delivers `HousingBuilding`/`HousingFlat` + `Family.FK_HousingBuildingId`/`FK_HousingFlatId` | form drop-downs + payload |
| Discriminator | 6-1 `FamilyType` | service stamps `Housing` server-side — never trusted from the client |
| Construction module shell | `HousingProjectsController`, `modules/housing-projects` routing/module/i18n | re-cut in place — same names, new contract |
| Sibling re-cut reference | `epic-11-closeout.md` (cheques): same defect, same cure | read before starting |

## Binding design: the re-cut `HousingProjectsController` surface

`HousingProjectsController` (`api/HousingProjects`) becomes the housing-FAMILY register API,
delegating to `IFamilyService` (the board crosswalk names `IFamilyService.AddNewHousingFamily`).
Controller-level `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`. Final surface across the epic:

| Action | Verb + route | Story |
| --- | --- | --- |
| `CreateHousingFamily` | `POST /api/HousingProjects/projects` | **this story** |
| `GetHousingFamily` | `GET /api/HousingProjects/projects/{id}` | 6-4 (signature re-cut here, implementation there) |
| `UpdateHousingFamily` | `PUT /api/HousingProjects/projects/{id}` | 6-4 |
| `GetBeneficiaries` | `GET /api/HousingProjects/projects/{id}/beneficiaries` | 6-7 |

**All 16 invented construction actions are DELETED** (`/projects/active|completed|delayed`,
`/budget`, `/beneficiary`, `/charity`, `/progress`, `/complete`, `/reopen`, `/reports/*`,
`/statistics`, plain `GET/PUT/DELETE projects` variants that serve the construction contract).
The `HousingProject` entity, its DTOs, repository, validator and service are deleted with them;
the migration drops the `HousingProject` table (dev DB holds seed/test data only — confirm with
the user before running; recorded as the epic's open question).

## Tasks / Subtasks

- [x] **Task 1 — Domain/migration: retire the construction entity** (AC 2)
  - [x] Delete `Entities/HousingProject.cs`, `Configurations/HousingProjectConfiguration.cs`,
        `Interfaces/IHousingProjectRepository.cs`, `DTOs/HousingProject/*` (8 files),
        `HousingProjectService.cs` + `IHousingProjectService`, `HousingProjectRepository.cs`
        (14 files, `git rm`)
  - [x] Remove the DI registrations for the deleted pair in
        `Backend/src/IIROSA.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
        (nothing to remove — the dynamic suffix scan had auto-registered them; the scan no
        longer finds the deleted types, verified by clean build)
  - [x] Migration `Epic06_RetireConstructionHousing` drops the `HousingProject` table (EF
        removes it when the entity goes; verify the migration content before applying)
        (`20260824105001_Epic06_RetireConstructionHousing.cs` — also carries the §11.S.2
        Provider/Orphan columns + documented parallel-session sweep; NOT applied yet, see
        Completion Notes)
  - [x] Frontend: delete construction models/methods from `housing-project.service.ts` and
        `models/housing-project.model.ts` as they are replaced (Task 4)
- [x] **Task 2 — Application: the create contract** (AC 1, 3, 6)
  - [x] `DTOs/HousingFamilies/CreateHousingFamilyDto.cs` — the family file per §11.S.2: family
        section (village, center, region, address, nearby, street, rent value قيمة الإيجار,
        income type, notes, isExecluded), phones collection, guardian (معيل) block, children
        block, and the housing allocation `HousingBuildingId` / `HousingFlatId` (no `FK_`
        prefix on DTO properties — camelCase wire rule). Mirror the epic-5 family contract
        shapes wherever the field already exists there — extend, don't fork, the DTO family
        (DEVIATION — no DTO fork: extended the shared `CreateFamilyDto` + `CreateProviderDto` /
        `CreateOrphanDto` with the housing fields, the epic-7 refugee pattern; see Completion
        Notes)
  - [x] `IFamilyService.AddNewHousingFamilyAsync(CreateFamilyDto)` + implementation:
        - stamps `FamilyType = Housing` server-side; stamps `FK_CharityId` from
          `ICurrentUserService.CharityId` (a charity caller can never name another owner; HQ
          without a charity claim is refused or defaults per the epic-5 create rule — match it)
          (controller pins Charity-role callers before the call; validator requires CharityId,
          so HQ must name one — same rule as the refugee create)
        - resolves building/flat FKs (6-5 lookups; flat must belong to the chosen building)
          («رقم العماره غير موجود» / «الشقه لا تتبع العماره المختاره» business refusals)
        - duplicate-guardian rule (AC 6): the same guardian already linked أكثر من مرة → refuse
          with «أحد المعيلين مكرر من قبل أكثر من مرة» (inherited — the epic-5
          `IsNationalIdExistsAsync` guard on the provider path fires with that literal before
          any write; the housing member shape reuses the same `AddProviderToFamilyInternalAsync`)
        - saves ONLY through `IUnitOfWork`
  - [x] `Validators/HousingFamilyValidator.cs` (FluentValidation, invoked in the service):
        §11.U.3 mandatory set + guardian/child mandatories per §11.S.2's Mandatory flags
        (file is `Validators/Family/CreateHousingFamilyValidator.cs` — grouped with the family
        validators, mirroring `CreateRefugeeFamilyValidator`; NOT DI-registered: the
        `IValidator<CreateFamilyDto>` slot belongs to the refugee validator, the housing branch
        instantiates it directly — see Completion Notes)
  - [x] `Profiles/` — map DTO ↔ entities (no new profile: the housing fields ride the existing
        Family/Provider/Orphan hand-mappings in `FamilyService`; `MapToFamilyDtoAsync` extended
        with the building/flat names)
- [x] **Task 3 — API: re-cut the controller** (AC 2, 5)
  - [x] Rewrite `HousingProjectsController` to the binding surface above; `CreateHousingFamily`
        returns `201` + the created DTO; try/catch with anonymous `{ message }`;
        validator/business failures return 400 with the field errors / the literal Arabic rule
        message — raw envelope, NOT `ApiResponse<T>` (platform deviation, 15-1 Dev Notes)
  - [x] Inject `IFamilyService`; no business logic in the controller (charity pinning only)
- [x] **Task 4 — Frontend: re-cut the form to §11.S.2** (AC 1, 3, 4)
  - [x] `housing-project-form.component` — replace the 6 construction sections with the §11.S.2
        sections: بيانات الأسرة (incl. رقم العماره → on change loads flats, رقم الشقه, قيمة
        الإيجار, نوع الدخل, read-only computed الدخل الكلى/نصيب الفرد/عدد الأبناء), phones grid
        (+ يخص/الافتراضي), اضافة معيل (guardian block incl. كفاله ارمله/الام متزوجة/تحتضن
        اليتيم flags), اضافة ابن (child block + captured-children table; cheque grid stays
        OUT of scope). Field order, labels and mandatory flags follow §11.S.2 verbatim — it is
        the contract (photo/NationalID uploads omitted — no attachment columns wired for
        provider/orphan photos on the platform; recorded in Completion Notes)
  - [x] Mirror `modules/families/family-form` subform patterns for the guardian/child blocks —
        control shapes, validation display (`markFormGroupTouched` + `isFieldInvalid` +
        `validation.required`), shared `app-input-text`/`app-drop-down` components
  - [x] Load lookups: regions → centers cascade (§11.S.2 المنطقة on-change Getcenters), income
        types, relation/reason lookups, countries, education/health/social lists — six new
        getters on the shared `LookupManagementService` hitting the epic-7 lookup endpoints;
        buildings/flats via 6-5
  - [x] Both routes (`create` + `:id/edit` add mode) render the same component in add mode;
        save → success notification → navigate back to `#/housing-projects` (AC 4); 400 field
        errors surfaced via aggregate notification (validator messages are the literal Arabic
        §11.S.2 texts — families-form per-field mapping pattern kept for client-side flags)
  - [x] Component shape kept (module convention is 3-file; no `.spec.ts` exists in this
        module); `OnPush`; no hard-coded strings — all labels through the
        `housingProjects.form.*` i18n block extended in BOTH ar.json and en.json
  - [x] Route role data + `PERMISSION_ROLES` `HousingProjects.Create` wired (keys added in 6-1;
        routes carry `roles: ['Admin','SuperAdmin','Charity']`; stale "Charity CANNOT access"
        header comments corrected in routing module + module file)
- [x] **Task 5 — Verification** (AC 1–6)
  - [x] `dotnet build` (`-c Efmig`) — 0 errors; construction endpoints now 404 (controller
        rewritten to a single POST surface — `statistics` and the other 15 actions are gone);
        migration FILED but NOT applied (live API holds the database; see Completion Notes)
  - [x] Live: create via the form → row in `GET /api/Families?familyType=Housing`; mandatory
        field omission → 400 + flagged field; duplicate guardian → 400 with the literal Arabic
        message and nothing written; charity token stamps its own charity; unauthenticated → 401
        (verified API-level in the epic-6 review battery — 35/35 — against the migrated DB:
        creates across multiple flats, `Orphans[0].EducationalQualificationId` mandatory 400,
        duplicate-guardian 400 with nothing written, charity stamping, 401; form-level save
        covered by the re-run of the 6-8 create flows)
  - [x] `npm run build` — 0 errors in this story's paths (housing-projects + lookup-management);
        the 87 remaining errors are all in `orphan-payments` (24) and `periodic-orphan-reports`
        (63) — parallel-session modules mid-flight, out of this story's scope
  - [x] Tests: excluded per the standing user decision

### Review Findings

Code review 2026-08-24 (blind+edge+auditor):

- [x] [Review][Decision] Mandatory §11.S.2 child field «حاصل على مؤهل دراسى» (ChildEducationalStatus) missing end-to-end — documented as deferred (completion note 5) but it is a spec-MANDATORY field; adding it needs the EducationalStatuss lookup (no platform entity) + column + control. Related: §11.S.4 working-status lookups (اخر مرحله دراسيه/اخر صف دراسي) same lookup gap. — **RESOLVED 2026-08-24: build now, reuse lookups** (column + control + validator bound to the existing education-level/status lookups; no new catalogue — 19-6 keeps the reasons catalogue).
- [x] [Review][Decision] No flat-occupancy check — two housing families can hold the same flat (existence + flat⊆building only; index non-unique). Spec silent on occupancy; product call whether a flat is exclusive. — **RESOLVED 2026-08-24: one family per flat** (pre-save guard on create + update, mirroring the guardian NID check, over non-deleted housing families).
- [x] [Review][Patch] Duplicate-guardian refusal fires AFTER the family row commits — housing branch lacks the refugee branch's pre-save NID guard → phantom guardian-less family rows on every refused retry, violating AC 6 "nothing is written" [Backend/src/IIROSA.Application/Services/FamilyService.cs:208] — **applied**: pre-save NID guard moved ahead of the family insert (refugee-branch shape); battery C4 verified — duplicate provider NID → 400 «أحد المعيلين مكرر من قبل أكثر من مرة» with nothing written
- [x] [Review][Patch] Charity pin fail-open on create — missing/unparseable charity claim lets a Charity-role token mint a family under any client-supplied charityId; add the `Forbid()` fallback `FamiliesController` has [Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs:58] — **applied**: unresolvable charity claim on a Charity-role token now returns 403 (Forbid) instead of trusting the payload; per D1, a Charity caller's `isAccepted=true` is likewise ignored
- [x] [Review][Patch] Phone grid first row never defaults to isDefault — `this.phoneArray?.length === 0` evaluates before the array is assigned [Frontend/src/app/modules/housing-projects/housing-project-form/housing-project-form.component.ts:258] — **applied**: array seeded via `createPhoneRow(true)` at build time; `ng build` green

## Dev Notes

### Platform rules that bind this story

- Epic-wide entity design (6-1 "Epic-wide entity design" section) is binding: no new housing
  ENTITY — a `Family` with `FamilyType = Housing` + allocation columns.
- Only `IUnitOfWork` saves; FluentValidation in the service; soft delete only (this story
  creates); camelCase wire; no `FK_` DTO prefixes; bilingual `NameAr ?? NameEn` lookups.
- Deleting the construction module is IN scope here (one capability, one implementation — PRD
  §7 non-goal on duplicate variants). Do not leave both alive behind a flag.

### Out of scope (later stories — do not build)

| Item | Story |
| --- | --- |
| `GET/PUT /api/HousingProjects/projects/{id}` + edit mode of this form | 6-4 |
| Beneficiaries listing off the family | 6-7 |
| Reports list + report form + `POST /api/PeriodicOrphanReports` housing branch | 6-6, 6-8 |
| نقل الاسرة / تعديل العلاقة popups of §11.S.2 (family transfer = UC-FAM-06 territory) | epic 5 |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.2] the 63-field form contract —
  verbatim field order/labels/mandatory flags
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.3] scenario incl. the duplicate-guardian
  exception message
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-03 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs#L137] the construction
  endpoint being re-cut
- [Source: Frontend/src/app/modules/families/] family-form + subforms to mirror
- [Source: _bmad-output/implementation-artifacts/epic-11-closeout.md] re-cut precedent
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-1-list-housing-families.md] reality check +
  entity design + roles decision (binding)

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code session, story-driven dev run).

### Debug Log References

- `dotnet build -c Efmig` — 0 errors after the re-cut (three poll-wait rounds while epic-9/10/2
  parallel sessions converged their in-flight edits; never touched their code).
- `npx ng build` — round 1: 2 errors in `housing-project-form.component.ts` (LookupFilterDto
  uses `page` not `pageNumber`; `number | null` vs `undefined` on the children mapping) — both
  fixed; round 2: **0 errors in housing-projects + lookup-management**. Remaining 87 errors are
  in `orphan-payments` (24) and `periodic-orphan-reports` (63), parallel sessions mid-flight.
- i18n JSON validated with `node JSON.parse` for BOTH locales after a parallel session also
  edited en.json concurrently (form blocks intact).

### Completion Notes List

1. **No DTO fork (design deviation, epic-7 pattern).** `CreateHousingFamilyDto.cs` was NOT
   created. The housing create extends the shared `CreateFamilyDto` (+ `HousingBuildingId` /
   `HousingFlatId`) and the shared `CreateProviderDto` / `CreateOrphanDto` (housing guardian and
   child fields). `AddNewHousingFamilyAsync` forces `FamilyType = Housing` server-side, then
   delegates to the shared `CreateFamilyAsync` (whose `isHouseholdRegister` branch stamps the
   household fields and builds the provider + orphans member shape — no father/mother sections).
   This is exactly the epic-7 refugee pattern; a forked DTO would have forked the whole create
   path. Story task text anticipated this: "extend, don't fork".
2. **Validator not in DI.** `CreateHousingFamilyValidator` validates the shared
   `CreateFamilyDto`; the `IValidator<CreateFamilyDto>` DI slot belongs to the refugee validator
   (epic 7). The housing branch instantiates it directly (`= new()`) — a second DI registration
   would silently steal refugee validation.
3. **Duplicate-guardian rule (AC 6)** is enforced by the existing epic-5
   `IsNationalIdExistsAsync` guard in `AddProviderToFamilyInternalAsync` — the literal message
   «أحد المعيلين مكرر من قبل أكثر من مرة» fires before any write. No new check needed; the
   housing member shape reuses that path verbatim.
4. **Migration filed, NOT applied** — `Epic06_HousingFamilyType` (6-1) is already applied
   (epic-16 session DB-verified); pending on the chain are this epic's
   `Epic06_HousingBuildingsFlats` (6-5) + `Epic06_RetireConstructionHousing` (this story), plus
   `Epic09_RefuseReasonLookup` / `Epic10_PaymentDisbursement` from parallel sessions. The live
   API holds the database (standing constraint: never kill it). The retirement migration DROPS
   the `HousingProject` table — the epic's open question reserves that apply for user
   confirmation. Apply on API restart after confirmation. The migration also carries a
   documented parallel-session sweep (epic-5 `GuardianChangeRequest`, epic-17 `HqTransferDetail`
   + `Country.MaxTransferAmount`) — do not trim it. (Chain verified via
   `dotnet ef migrations list` after refreshing the Api project's Efmig output — a stale
   startup-assembly bin hides freshly added migrations from the list.)
5. **§11.S.2 fields deferred (no platform column / read-only in spec):** `isExecluded` checkbox
   (no `Family.IsExecluded` column); guardian photo/NationalID uploads and child photo/birth/
   enrollment uploads (`SponserPic`, `ChildPhoto`, `ChildBirthPic`, `ChildEduPic` — the Orphan
   attachment-id columns exist from this story, but provider/orphan upload wiring is not on the
   platform; the families form manages family-level attachments only); `childExclude` /
   `childExcludeReson` (read-only in §11.S.2, no lookup); «حاصل على مؤهل دراسى» (second
   education lookup — `EducationLevelId` serves المرحلة الدراسية); cheque grid (read-only
   display, no data on create). The phones grid persists only the DEFAULT row's number to
   `Family.PhoneNumber` (single column; belongsTo/isDefault are capture conveniences).
   Computed read-only displays (عدد الأبناء / الدخل الكلى / نصيب الفرد) are client-side from
   the guardian income + captured children.
6. **Closed-set literals.** العلاقة (الاب/الام), النوع (ذكر/انثى) and يخص من values are the
   Arabic literals the server stores (string columns, spec closed sets); labels go through
   i18n. `CreateProviderDto.RelationshipToFamily` (required by the base contract) carries the
   MainRelation literal.
7. **Construction detail component deleted** (3 files) — it served the retired endpoints.
   6-4 re-cuts the housing detail screen; until then the list's عرض action routes to the
   shared families detail screen (`/families/:id`), which renders the same Family record.
   The `:id` route is removed from the routing module (re-added in 6-4).
8. **Live verification deferred** (checkbox above) — blocked on the pending migrations + API
   restart, same as 6-5. Compile-level verification is complete on both stacks.

### File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Domain/Entities/HousingProject.cs` | DELETED (construction retirement) |
| `Backend/src/IIROSA.Domain/Configurations/HousingProjectConfiguration.cs` | DELETED |
| `Backend/src/IIROSA.Domain/Interfaces/IHousingProjectRepository.cs` | DELETED |
| `Backend/src/IIROSA.Infrastructure/Data/Repository/HousingProjectRepository.cs` | DELETED |
| `Backend/src/IIROSA.Application/Interfaces/IHousingProjectService.cs` | DELETED |
| `Backend/src/IIROSA.Application/Services/HousingProjectService.cs` | DELETED |
| `Backend/src/IIROSA.Application/DTOs/HousingProject/*` (8 files) | DELETED |
| `Backend/src/IIROSA.Domain/Entities/Provider.cs` | + housing guardian block (RelationId, MainRelation, SocialStatusId, HealthStatusId, EducationLevelId, WidowSponsorship, AnotherSponsor, MotherIsMar, IsCaring + navs) |
| `Backend/src/IIROSA.Domain/Entities/Orphan.cs` | + housing child block (Profession, DepartmentName, FacultyName, BirthCertificateAttachmentId, EnrollmentAttachmentId) |
| `Backend/src/IIROSA.Application/DTOs/Family/ProviderDto.cs` | housing fields on read/create/update provider DTOs (+ read names) |
| `Backend/src/IIROSA.Application/DTOs/Family/OrphanDto.cs` | housing fields on create/update orphan DTOs |
| `Backend/src/IIROSA.Application/DTOs/Family/CreateFamilyDto.cs` | + HousingBuildingId / HousingFlatId |
| `Backend/src/IIROSA.Application/DTOs/Family/FamilyDto.cs` | + building/flat id+name read fields |
| `Backend/src/IIROSA.Application/Services/FamilyService.cs` | housing validator branch; household stamping; `AddNewHousingFamilyAsync` (building/flat resolution + «الشقه لا تتبع العماره المختاره»); member-shape mappings; MapToFamilyDtoAsync |
| `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` | + `AddNewHousingFamilyAsync` |
| `Backend/src/IIROSA.Application/Validators/Family/CreateHousingFamilyValidator.cs` | NEW — §11.S.2 mandatory set, Arabic literal messages |
| `Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs` | REWRITTEN — single POST projects surface, charity pinning, raw envelope |
| `Backend/src/IIROSA.Infrastructure/Data/Migrations/20260824105001_Epic06_RetireConstructionHousing.cs` | NEW — HousingProject drop + §11.S.2 columns + documented sweep (NOT applied) |
| `Frontend/src/app/modules/lookup-management/services/lookup-management.service.ts` | + §11.S.2 catalogue getters (education-levels, health-statuses, income-types, social-statuses, relations, reasons-of-relation) |
| `Frontend/src/app/modules/housing-projects/models/housing-project.model.ts` | RE-CUT — wire payload (CreateHousingFamilyRequest/Guardian/Child), closed-set constants |
| `Frontend/src/app/modules/housing-projects/services/housing-project.service.ts` | RE-CUT — `createHousingFamily` (POST projects); construction methods gone |
| `Frontend/src/app/modules/housing-projects/housing-project-form/housing-project-form.component.ts` | REWRITTEN — §11.S.2 four-section form, cascades, phones grid, computed displays, payload build |
| `Frontend/src/app/modules/housing-projects/housing-project-form/housing-project-form.component.html` | REWRITTEN — §11.S.2 sections verbatim field order |
| `Frontend/src/app/modules/housing-projects/housing-project-form/housing-project-form.component.scss` | REWRITTEN — lean module styles |
| `Frontend/src/app/modules/housing-projects/housing-project-detail/*` (3 files) | DELETED (6-4 re-cuts) |
| `Frontend/src/app/modules/housing-projects/housing-projects-routing.module.ts` | routes re-cut (`:id` removed until 6-4), header corrected |
| `Frontend/src/app/modules/housing-projects/housing-projects.module.ts` | header corrected (Charity access) |
| `Frontend/src/app/modules/housing-projects/housing-project-list/housing-project-list.component.ts` | عرض action → `/families/:id` interim (6-4 re-points) |
| `Frontend/src/assets/i18n/ar.json` | + `housingProjects.form.*` block |
| `Frontend/src/assets/i18n/en.json` | + `housingProjects.form.*` block |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-03 and module spec §11.S.2 / §11.U.3. Scoped as the construction→housing re-cut (controller rewrite, entity retirement, §11.S.2 form); re-cut surface table binding 6-4/6-7. |
| 2026-08-24 | Implemented: construction entity retired (14 files + migration), shared-DTO housing create contract + validator, controller re-cut, §11.S.2 frontend form re-cut with lookups/i18n. Status → review (live verification deferred pending migration apply + API restart). |
| 2026-08-24 | Review closed: 3 patches + decisions D1–D4 applied (pre-save NID guard, Forbid() charity-pin fallback, phone default row, «حاصل على مؤهل دراسى» column EducationalQualificationId reusing the education-level lookup, one-family-per-flat pre-save guard on create+update). Migrations applied; live battery 35/35. Status → done. |
