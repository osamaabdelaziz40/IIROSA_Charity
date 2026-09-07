# Story 7-3: Register a refugee family

| Field | Value |
| --- | --- |
| Story key | `7-3-register-a-refugee-family` |
| Epic | EP-07 — Refugee Families (الاسر اللاجئة) |
| Use case | UC-REF-03 — اضافة أسرة لاجئة |
| Priority / size | Should · 10 points |
| Specification | `docs/Modules/12-UC-REF-Refugee-Families.md` (§12.S.2 screen, §12.U.3 scenario) |
| Route | `#/families/refugees/create` (board comment names `#/families/refugees/:id/edit` — the legacy screen id; the create route is the annex 12.A realisation) |
| Endpoint | `POST /api/Families` |
| Depends on | 7-1 (discriminator + list screen the record must appear in) |
| Roles | Charity (primary) → controller keeps `SuperAdmin,Admin,Charity`; an HQ caller must supply the target `charityId`, a Charity caller is pinned to its own |

## Status

review

## Story

As a charity user, I want to be able to register a refugee family اضافة أسرة لاجئة, so that the
register reflects reality as soon as the fact is known.

## Acceptance Criteria

1. Given a charity user with an active session on the refugee form, when the actor presses «حفظ»
   with valid input, then a new record exists, owned by the charity of the creating user
   (`FamilyType = Refugee`), and appears in the list screen of the module.
2. Given the request is accepted, when it is served, then it is handled by `POST /api/Families`
   and the response is rendered on the screen without a page reload.
3. Given a mandatory field listed in the §12.S.2 specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged (client-side flag + server-side
   FluentValidation refusal).
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.
6. Given the business rule behind «أحد المعيلين مكرر من قبل أكثر من مرة» is broken (a provider
   national id already linked as provider to another family, or twice in this family), when the
   operation is attempted, then it is refused with that message and nothing is written.

**Definition of done:** the screen fields of §12.S.2 are implemented with their mandatory flags and
lookups; the scenario of §12.U.3 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Reality check: the family create path is LIVE — this story adds the refugee contract on top

`POST /api/Families` (`FamiliesController.cs:99` → `FamilyService.CreateFamilyAsync`) exists and
works for regular families (5-3 done). The families frontend already has member-capture
subcomponents (`father-form`, `mother-form`, `relatives-form`) and per-member endpoints
(`POST /api/Families/{familyId}/father|mother|relatives|orphans|provider` — all live). **No
refugee-specific columns, DTOs, validators, or lookups exist** — the §12.S.2 contract (48 fields)
needs: household columns on `Family`, six new lookups, member extensions, and the
duplicate-provider rule.

## Binding design: the refugee contract (decided here, referenced by 7-4)

**One superset contract, not a parallel DTO tree.** `CreateFamilyDto`/`UpdateFamilyDto` gain
`FamilyType` (string on the wire, parsed to the shared `Domain/Enums/FamilyType` enum in the
service — 6-1 convention) + the nullable refugee fields below. A dedicated
`CreateRefugeeFamilyValidator` (`AbstractValidator<CreateFamilyDto>`) runs in the service when
`FamilyType == Refugee` — typed DTO in, typed DTO out; no `JObject` (prd.md §7), no
`RefugeeFamiliesController`.

### Household fields → `Family` columns (migration `Epic7_RefugeeContract`)

| §12.S.2 field (label) | Binding | Mandatory | Notes |
| --- | --- | --- | --- |
| القرية / الحي | existing `CityVillage` | ✔ | — |
| المنطقة /المحافظة | NEW `int? RegionId` + `virtual Region? Region` | ✔ | cascade loads Centers |
| المركز/ المدينة | NEW `int? CenterId` + `virtual Center? Center` | ✔ | `GET /api/LookupManagement/centers/by-region/{regionId}` |
| بجوار | NEW `string? NearBy` (100) | — | — |
| الشارع | NEW `string? Street` (100) | — | — |
| العنوان التفصيلى | existing `Address` | ✔ | — |
| قيمة الإيجار | NEW `decimal? RentAmount` (18,2) | ✔ | — |
| ملكية السكن | NEW `int? HouseOwnershipId` + nav | ✔ | new lookup HouseOwnership; rent field shown/hidden by ownership (legacy `DisplayRentValue()`) |
| حالة محتويات السكن | NEW `int? HouseStatusId` + nav | ✔ | new lookup HouseStatus |
| نوع السكن | existing `HousingTypeId` + nav | ✔ | lookup exists |
| نوع الدخل | NEW `int? IncomeTypeId` + nav | ✔ | new lookup IncomeType |
| الدخل الكلى | existing `MonthlyIncome` | read-only | total of member incomes |
| نصيب الفرد | DTO-computed `PerMemberShare = MonthlyIncome / FamilyMembersCount` | read-only | NOT a column |
| عدد الأبناء | existing `FamilyMembersCount` | read-only | — |
| ملاحظات الباحث | existing `Notes` | — | — |
| إسم الأسرة | derived `HeadOfFamily` | read-only | father/provider name; read-only on form |

All new columns are nullable — a `Regular` family row is untouched; one table, no join.

### Six new lookups (`LookupEntity`, `MappingDefaults.LOOKUP_SCHEMA`)

`HouseOwnership`, `HouseStatus`, `IncomeType`, `SocialStatus`, `Relation`, `ReasonOfRel` —
entities in `IIROSA.Domain/Entities/Lookups/`, configurations, seeding (bilingual `NameAr`/`NameEn`;
seed from the legacy option lists: ownerships ملك/إيجار/…, statuses, income types, relations,
reasons-of-relation), and read exposure via `LookupManagementController` following the existing
lookup endpoint pattern (reads for authenticated users, writes `SuperAdminOnly`). Registers/
Centers/Countries/HealthStatus already exist — reuse, do not recreate.

### Members → existing entities + endpoints (live, do not fork)

| §12.S.2 section | Entity + endpoint (existing) | Extension in this story |
| --- | --- | --- |
| اضافة معيل (provider: 4-part name, nationality, DOB, passport/NId, death date + سبب الوفاة, relation نوعها/العلاقة, reason السبب) | `Provider` via `POST /api/Families/{familyId}/provider` | add nullable `DateOfBirth`, `NationalityCountryId` (+`Country` nav), `IsAlive`, `DeathDate`, `DeathReason` (string — closed set طبيعية/مرض/حادث from a static dropdown), `ReasonOfRelationId` (+`ReasonOfRel` nav). Relation name (نوعها) → existing `RelationshipToFamily` string, value chosen from the Relation lookup |
| اضافة ابن (child: name, DOB, NId max 14, photo, health + social status, gender) | `Orphan` via `POST /api/Families/{familyId}/orphans` | add `int? SocialStatusId` (+nav). Photo → existing `PhotoAttachmentId` via the shared attachment component |
| اضافة مرافق (accompany: same shape as child minus code) | `Relative` via `POST /api/Families/{familyId}/relatives` | none — a مرافق is a Relative with `RelationshipType = 'Accompany'` (constant on the frontend); no schema change |

4-part names (الاول/السانى/الثالث/الرباعي) are four inputs joined with spaces into the existing
`FullName` single column — matches the father-form precedent; do not add name-part columns.

### Duplicate-provider rule (AC 6)

In `FamilyService`, before persisting a provider add (refugee or not): look for another
non-deleted `Provider` with the same `NationalId` already linked to a different family (or a
second occurrence in this family) → throw `BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة")`
(Application-layer exception, maps to 400/409 per architecture.md §5.1); nothing written. The
message is the literal legacy string — do not translate or reword it.

## Tasks / Subtasks

- [x] **Task 1 — Domain + lookups + migration** (AC 1)
  - [x] `Family.cs`: add the 8 nullable household columns + `Region`/`Center`/`HouseOwnership`/
        `HouseStatus`/`IncomeType` navigations per the table above (audit fields inherited;
        `MappingDefaults` schemas; no `DbSet`)
  - [x] `Provider.cs`: add the 6 nullable provider columns + navigations; `Orphan.cs`: add
        `SocialStatusId` + navigation
  - [x] Six new `LookupEntity` classes + configurations + seed data (find the existing seed
        mechanism in `Infrastructure` — the Technical-Support lookups seeding is the sanctioned
        precedent for lookup seeding)
  - [x] Migration `Epic7_RefugeeContract` (all nullable → no backfill needed) + database update —
        **absorbed, not minted**: the refugee contract shipped inside epic-6's already-applied
        `20260824102843_Epic06_HousingBuildingsFlats` (see Dev Agent Record)
- [x] **Task 2 — Application layer** (AC 1, 3, 6)
  - [x] `CreateFamilyDto`/`UpdateFamilyDto`/`FamilyDto`: add `FamilyType` + the refugee fields +
        resolved `RegionName`/`CenterName`/`HouseOwnershipName`/`HouseStatusName`/`IncomeTypeName`
        (`NameAr ?? NameEn`) + `PerMemberShare` (computed, get-only)
  - [x] `Validators/Family/CreateRefugeeFamilyValidator.cs` (+ `UpdateRefugeeFamilyValidator` for
        7-4): mandatory rules for القرية/المنطقة/المركز/العنوان/قيمة الإيجار/ملكية السكن/حالة
        المحتويات/نوع السكن/نوع الدخل + provider name-part/NId/DOB/nationality rules + child
        rules (NId max 14). Invoked from `FamilyService` when `FamilyType == Refugee`
        (SeasonalAid validators are the pattern); the existing regular-family path keeps its
        current behaviour
  - [x] `FamilyService.CreateFamilyAsync`: stamp `FamilyType`, stamp `FK_CharityId` from the
        caller (Charity role: own claim; HQ: require explicit `charityId` in the DTO, refuse with
        a validation error if absent); duplicate-provider guard per the design above
  - [x] Lookup exposure: add the six lookups to `LookupManagementController` reads following the
        existing pattern (cascading centers-by-region endpoint already exists — reuse) — plus a
        seventh read `housing-types` (the spec's "lookup exists" assumption was wrong: no
        HousingType lookup endpoint existed; the full 10-touchpoint stack was added)
- [x] **Task 3 — API layer** (AC 2, 5)
  - [x] `POST /api/Families` — no new route; the refugee path returns field-level validation
        errors in the controller's existing error shape (`FluentValidation.ValidationException` +
        `BusinessException` catch blocks added to Create/Update — they previously leaked as 500s)
  - [x] Roles unchanged (`SuperAdmin,Admin,Charity`) — server-side; the menu gate is convenience
- [x] **Task 4 — Refugee form screen** (AC 1, 3, 4)
  - [x] `Frontend/src/app/modules/families/refugee-family-form/` — 4-file
        `RefugeeFamilyFormComponent` (standalone/OnPush, lazy via the families routing module);
        route `refugees/create` ABOVE `refugees`/`':id'` in `families-routing.module.ts`
        (ordering landmine from 7-1), permission `Families.Create`; the same component serves
        `refugees/:id/edit` in 7-4
  - [x] Sections per §12.S.2: بيانات الأسرة (household fields; المنطقة→المركز cascade clears
        the centre VALUE and reloads its options), اضافة معيل / اضافة ابن / اضافة مرافق as
        staged FormArray rows (`app-input-text` / `app-drop-down`)
  - [x] Members ride the SINGLE `POST /api/Families` payload (`provider` / `orphans` /
        `relatives`) — no per-member follow-up calls, so a refused create persists nothing
        (stronger than the staged design above; the server refuses the whole batch — AC 6 holds
        atomically)
  - [x] Mandatory flags per §12.S.2; client-side flagging before the call; server ModelState /
        validation errors surfaced via `modelStateErrors` (Arabic messages shown verbatim)
  - [x] On success: notification + navigate to `/families/refugees` (AC 4); the list toolbar
        gained a permission-gated «تسجيل أسرة لاجئة» button
  - [x] نقل الاسرة (transfer) button: NOT rendered (5-6 scope). Cheque grid: not built (7-4)
- [x] **Task 5 — Menu + i18n**
  - [x] Route + toolbar entry live at `#/families/refugees/create`
  - [x] i18n: full `families.refugee*` form block (all field labels, section titles, gender /
        death-reason option labels, commands, validation messages) in **both** `ar.json` and
        `en.json`
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Build + live instance from isolated output (MSB3027 = live-API lock; user's API untouched)
  - [x] Live script `.tmp-7-3-verify.mjs` — **25/25 PASS** (matrix in Dev Agent Record); rows
        swept from the DB afterwards (0 leftovers)
  - [x] Catalogue smoke `.tmp-7-3-housing-types-smoke.mjs` — **7/7 PASS** (incl. new housing-types)
  - [x] `cd Frontend && npm run build` — epic-7 files **0 TypeScript errors** across consecutive
        full builds; the build itself is currently red ONLY from concurrent sessions' in-flight
        edits (housing-projects `loadFamily`, orphan-payments-detail template members) — not from
        this story's files

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] NId semantics: §12.S.2 specifies «رقم جواز السفر» (passport) for معيل/أبناء; implementation labels and keys on «الرقم القومي» (national ID) — RESOLVED 2026-08-24: keep national-ID semantics and label; accepted deviation from §12.S.2 (duplicate-معيل rule stays keyed on NationalId)
- [x] [Review][Decision] Four mandatory name-part fields (الاسم الاول/الثانى/الثالث/الرباعي) collapsed into a single `fullName` input (father-form precedent, single storage column) — RESOLVED 2026-08-24: keep the single input with the ≥2-word server check; accepted deviation (platform-consistent)
- [x] [Review][Decision] Optional §12.S.2 fields dropped unrecorded: ابن الصورة photo upload (7-3 binding promised the shared attachment component) and معيل إسم الأسرة (`Provider` has no FamilyName column) — RESOLVED 2026-08-24: record the gap; implement in a dedicated follow-up story (photo via shared attachment component; add `Provider.FamilyName` column + migration only if that story wants it)
- [x] [Review][Patch] (fixed 2026-08-24) [High] Create is fail-open for a Charity token with null/unparseable charity claim — `dto.CharityId` pinned only `if (userCharityId.HasValue)`; add the D4 guard used by the orphan endpoints [Backend/src/IIROSA.Api/Controllers/FamiliesController.cs:315]
- [x] [Review][Patch] (fixed 2026-08-24) [High] Mandatory الحالة الصحية missing from child AND companion rows — `Orphan.HealthStatusId` and `Relative.HealthStatusId` both exist; add control + `getHealthStatuses()` + payloads + detail columns + i18n keys [refugee-family-form.component.html]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] `Enum.TryParse` result discarded on create — unparseable `familyType` persists discriminator 0 (row vanishes from every register filter) [Backend/src/IIROSA.Application/Services/FamilyService.cs:126-130]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] معيل block not mandatory on refugee create — all provider rules inside `When(Provider != null)`; guardian-less refugee family persists [Backend/src/IIROSA.Application/Validators/Family/CreateRefugeeFamilyValidator.cs:51]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Child `nationalId` maxLength-only, companion unvalidated — §12.S.2 marks both mandatory, max 14 (create + edit row builders) [refugee-family-form.component.ts addChildRow/addCompanionRow]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Mandatory provider العلاقة control (الاب/الام/علاقة أخرى) absent — `Provider.MainRelation` column exists unused; «علاقة أخرى» reveals نوعها per spec `OtherRelations()`
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Read-only displays الدخل الكلى/نصيب الفرد/عدد الأبناء rendered nowhere (`perMemberShare` modeled but unbound) — add to detail + i18n
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Catalogue GET failures silently empty dropdowns; failed `getRelations()` makes edit PUT send `relationshipToFamily: undefined`, wiping stored data — surface errors + `pendingRelationName` fallback in `optionName` [refugee-family-form.component.ts]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] Validator NRE on null `Provider.FullName` — `Must` runs after failed `NotEmpty` (FV cascade Continue) → 500 [Backend/src/IIROSA.Application/Validators/Family/CreateRefugeeFamilyValidator.cs:55]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] `IValidator<CreateFamilyDto>` DI-ambiguous — `CreateHousingFamilyValidator` also auto-registered; refugee-wins is registration-order luck. Deterministic registration in `ServiceCollectionExtensions`
- [x] [Review][Patch] (fixed 2026-08-24) [Low] `save()` create subscribe lacks `takeUntil(destroy$)` [refugee-family-form.component.ts:580]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] `trackByIndex` on removable FormArray rows — track by the hidden `id` control instead
- [x] [Review][Patch] (fixed 2026-08-24) [Low] «إضافة أسرة لاجئة» menu entry lacks a `Families.Create` gate (inherits `Families.View`) [Frontend/src/app/layouts/main-layout/main-layout.component.html]
- [x] [Review][Defer] Refugee create sends unvalidated household lookup ids — FK violation surfaces as 500 not 400; deferred (error-shape nicety, cross-registers)
- [x] [Review][Defer] `familyType=Housing` via generic `POST /api/Families` bypasses building/flat allocation validation — deferred, owned by epic 6 (housing)

## Dev Notes

### Platform rules that bind this story

- All 7-1 rulings carry over: camelCase wire, no `ApiResponse<T>`, no `data-list`, no `FK_`-prefixed
  DTO names, soft-delete global filter, scoping via claims never body.
- FluentValidation runs in the **service layer** — the controller stays thin (architecture.md §5).
- Only `IUnitOfWork` saves; the member-add sequence goes through the existing per-member service
  methods — do not bypass them with direct repository writes.
- `BusinessException` / `ValidationException(List<string>)` / `NotFoundException` are the
  Application-layer exception vocabulary (architecture.md §5.1) — use them, don't invent new ones.
- Select2 stable-options landmine and cascade-clearing pattern from 7-2 apply to every dropdown
  on this form.
- Known pre-existing debt you must NOT churn: duplicate service pairs
  (`LookupManagementService` / `LookupManagementManagementService`), stray `Backend/Backend/**`
  trees — work in `Backend/src/**` only.

### Out of scope (later stories / other epics — do not build)

| Item | Story |
| --- | --- |
| `refugees/:id` + `refugees/:id/edit` routes, edit mode, detail view, cheque grid wiring | 7-4 |
| تعديل العلاقة / ChangeCurrentSponsor popups (guardian-change machinery) | 5-8…5-10 |
| نقل الاسرة (move family to another charity) | 5-6 |
| سبب الاستبعاد (childExcludeResons) — read-only field fed by orphan coding | epic 8 |
| Bank-per-nationality dropdown from the legacy form | epic 11 (cheques) |
| Multi-row phones/incomes (legacy AddphoneNumber/AddIncomeType icons) — single `PhoneNumber` + single `IncomeTypeId` implement the §12.S.2 field table; log a correct-course note if HQ wants rows | — |

### References

- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.S.2] the 48-field screen contract, grid,
  31 commands (scope trims above)
- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.U.3] scenario incl. the literal
  «أحد المعيلين مكرر من قبل أكثر من مرة» exception
- [Source: _bmad-output/planning-artifacts/epics.md#3.7] US-REF-03 acceptance criteria
- [Source: Backend/src/IIROSA.Domain/Entities/Family.cs] household columns to extend;
  [Provider.cs] / [Orphan.cs] member extensions
- [Source: Backend/src/IIROSA.Application/Validators/SeasonalAid/SeasonalAidCampaignValidators.cs]
  validator pattern to copy
- [Source: Frontend/src/app/modules/families/components/father-form/] member sub-form structure to
  mirror
- [Source: _bmad-output/implementation-artifacts/epic-7-refugee-families/7-1-list-refugee-families.md] discriminator
  design and platform rulings this story builds on

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code CLI session, `/bmad-dev-story` 7-1 → 7-4 sweep).

### Debug Log References

- Live verification: `http://localhost:60961` instance built to `Backend/.tmp-api-bin` (user's
  own API on its port was never touched; MSB3027 copy-lock worked around with `-o .tmp-api-bin`).
- `.tmp-7-3-verify.mjs` (repo root) — final matrix **25/25 PASS**: login; six catalogue reads;
  refugee create 201 with echo checks (familyType=Refugee, household fields, resolved
  Region/Center/HouseOwnership/HouseStatus/IncomeType names, nearBy/street/cityVillage, provider
  refugee fields incl. reasonOfRelationName, orphan socialStatusName); duplicate provider NID →
  400 + literal «أحد المعيلين مكرر من قبل أكثر من مرة» + nothing persisted; missing CenterId →
  400 Arabic «المركز/ المدينة مطلوب»; HQ without charityId → 400 "Charity is required"; rent 0 →
  400; regular-create regression → 201 typed Regular with father resolved.
- `.tmp-7-3-housing-types-smoke.mjs` (`Backend/`) — **7/7 PASS**: housing-types (new, 5 rows),
  house-ownerships (3), house-statuses (4), income-types (4), social-statuses (4), relations (3),
  reasons-of-relation (4).
- Post-run DB sweep (sqlcmd, `SET QUOTED_IDENTIFIER ON; SET ARITHABORT ON`): 2 families + 1
  father + 1 mother + 1 provider + 2 orphans deleted; marker count 0.
- Frontend: consecutive full `npm run build` runs grep'd for epic-7 path segments — 0 errors.

### Completion Notes List

1. **Migration absorbed, never minted.** Epic-6's concurrent migration
   `20260824102843_Epic06_HousingBuildingsFlats` (already applied) swept this story's refugee
   columns when it shipped — minting `Epic7_RefugeeContract` separately produced a duplicate /
   misattributed migration twice. Resolution: no epic-7 migration exists; the contract rides in
   the applied epic-6 chain; the model snapshot matches the last applied migration
   (`20260824105001_Epic06_RetireConstructionHousing`). Lesson recorded: re-check the migrations
   folder + `__EFMigrationsHistory` before minting when other sessions share the model, and never
   trust `dotnet ef --no-build` against a stale DLL.
2. **Seventh lookup (HousingType) was missing entirely.** The story assumed "lookup exists"; it
   existed as an entity but had no repository/service/DTO/DI/endpoint. The full per-lookup stack
   (10 touchpoints) was added and exposed as `GET /api/LookupManagement/housing-types`, plus the
   frontend `getHousingTypes()` getter.
3. **Seeding is existence-guarded** — HouseOwnership(3)/HouseStatus(4)/IncomeType(4)/
   SocialStatus(4)/Relation(3)/ReasonOfRel(4)/HousingType(5) rows were seeded manually with the
   same guards the startup seeder uses, so re-running the seeder is a no-op.
4. **Atomic create beats staged member calls.** The design sketched family-then-per-member
   persistence; the implemented path sends provider/orphans/relatives inside the one
   `POST /api/Families` (all three member blocks were already supported by `CreateFamilyAsync`),
   and the duplicate-provider guard runs BEFORE the first `SaveChanges` — a refused create
   writes nothing (AC 6 atomic; the partial-failure pathway in the task list is moot).
5. **500-leak fix in the controller**: `FluentValidation.ValidationException` and
   `BusinessException` catch blocks added to `FamiliesController.CreateFamily`/`UpdateFamily` —
   without them every §12.S.2 refusal surfaced as a 500 with no field errors.
6. **Resolved names needed includes**: `IncludeNavigationProperties` extended on
   Family/Provider/Orphan repositories (+`OrphanListDto.SocialStatusId/Name`) so the create
   echo returns RegionName/CenterName/…/SocialStatusName instead of nulls.
7. **Companion rows are stricter than the spec's optionality suggests**: the shared
   `CreateRelativeDto` is `[Required]` on `Gender` and `DateOfBirth`, so the مرافق row collects
   both (the form initially sent `gender: ''` which the server would refuse); the relation label
   rides `RelationshipType='accompany'` with the free-text صلة القرابة in `notes`.
8. **قيمة الإيجار is unconditionally mandatory** (server validates it flat); the legacy ASP form
   hid it for non-rented ownership — this form keeps it visible per the spec's flag table.
9. **Concurrent-session note for the reviewer**: `npm run build` is red at story-close time from
   OTHER sessions' in-flight edits (`housing-project-form.loadFamily`, `orphan-payment-detail`
   template members). Epic-7's own files compile clean in every run — verified by grepping each
   full build's error list for `refugee|families|lookup-management` (0 hits, consecutive runs).

### File List

**Backend — Domain / Infrastructure**
- `IIROSA.Domain/Entities/Family.cs` — refugee household columns + navigations (prior session)
- `IIROSA.Domain/Entities/Provider.cs` — refugee provider columns (prior session)
- `IIROSA.Domain/Entities/Orphan.cs` — `SocialStatusId` + navigation (prior session)
- `IIROSA.Domain/Entities/Lookups/HousingType.cs` (+ the six §12.S.2 lookups — prior session)
- `IIROSA.Domain/Configurations/…` — lookup configurations incl. HousingType
- `IIROSA.Domain/Interfaces/ILookupRepository.cs` — `IHousingTypeRepository`
- `Infrastructure/Data/Repository/LookupRepository.cs` — `HousingTypeRepository`
- `Infrastructure/Data/Repository/{Family,Provider,Orphan}Repository.cs` — refugee navs included
- `Infrastructure/Extensions/ServiceCollectionExtensions.cs` — HousingType service + repo DI
- `Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` — reverted my stray mint; the
  applied chain carries the contract

**Backend — Application / API**
- `Application/DTOs/Family/{FamilyDto,ProviderDto,OrphanDto}.cs` — refugee fields, resolved
  names, `PerMemberShare`, `SocialStatusId/Name`
- `Application/DTOs/LookupManagement/LookupDtos.cs` — HousingType DTO triple (+ six lookups)
- `Application/Interfaces/ILookupService.cs` — `IHousingTypeService` (+ six services)
- `Application/Interfaces/IFamilyService.cs` — (unchanged surface, refugee branch internal)
- `Application/Services/LookupManagementService.cs` — `HousingTypeService` (+ six services)
- `Application/Services/FamilyService.cs` — refugee branch: duplicate-provider pre-save guard,
  charity stamping, re-fetch echo
- `Application/Validators/Family/CreateRefugeeFamilyValidator.cs` — §12.S.2 mandatory flags
- `Application/Profiles/{FamilyProfile,LookupProfile}.cs` — refugee + HousingType maps
- `Api/Controllers/FamiliesController.cs` — ValidationException/BusinessException catches
- `Api/Controllers/LookupManagementController.cs` — seven refugee catalogue GETs incl.
  `housing-types`

**Frontend**
- `modules/families/refugee-family-form/` — NEW 4-file `RefugeeFamilyFormComponent`
  (`.ts`/`.html`/`.scss`/`.spec.ts`)
- `modules/families/refugee-family-list/` — toolbar «تسجيل أسرة لاجئة» (permission-gated),
  `canCreate`, backfilled `.spec.ts` (7-1/7-2 4-file shape)
- `modules/families/families-routing.module.ts` — `refugees/create` route (above `':id'`)
- `modules/families/models/family.model.ts` — refugee blocks on Family/Create/Update DTOs,
  `relationshipToFamily`, `headOfFamily`, companion typing
- `modules/lookup-management/services/lookup-management.service.ts` — `getHousingTypes()` (+ the
  six getters prior session)
- `assets/i18n/{ar,en}.json` — full `families.refugee*` form key block

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-REF-03 and module spec §12.S.2 / §12.U.3; refugee contract mapped onto the live families entities with six new lookups and the duplicate-provider rule bound. |
| 2026-08-24 | Implementation review-completed: backend live-verified 25/25 + catalogues 7/7; refugee form screen (4-file), route, toolbar wiring, i18n (ar+en); HousingType 10-touchpoint stack + `housing-types` endpoint; controller 500-leak fix; migration-absorption recorded. Status → review. |
