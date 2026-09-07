# Story 7-4: View / update a refugee family

| Field | Value |
| --- | --- |
| Story key | `7-4-view-update-a-refugee-family` |
| Epic | EP-07 — Refugee Families (الاسر اللاجئة) |
| Use case | UC-REF-04 — بيانات الأسرة اللاجئة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/12-UC-REF-Refugee-Families.md` (§12.S.2 screen, §12.U.4 scenario) |
| Route | `#/families/refugees/:id` (view) · `#/families/refugees/:id/edit` (update) |
| Endpoint | `GET /api/Families/{id}` · `PUT /api/Families/{id}` |
| Depends on | 7-1…7-3 (discriminator, list, search, refugee contract + form component) |
| Roles | Charity, HQ roles → `SuperAdmin`, `Admin`, `Charity` |

## Status

review

## Story

As a charity user, I want to be able to view / update a refugee family بيانات الأسرة اللاجئة, so
that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then the stored record carries the new values; no other record is affected.
2. Given the request is accepted, when it is served, then it is handled by `GET /api/Families/{id}`
   (load) and the update is persisted through the existing `PUT /api/Families/{id}`, rendered on
   the screen without a page reload.
3. Given a mandatory field listed in the §12.S.2 specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged (client flag + server refusal).
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected; given an HQ role, when an explicit charity id is
   supplied, then the function operates on that charity's data.
6. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §12.S.2 are implemented with their mandatory flags and
lookups in view and edit mode; the scenario of §12.U.4 passes end to end; scoping is enforced
server-side.

## Reality check: both endpoints are LIVE — this story is load-into-form, edit mode, and detail view

`GET /api/Families/{id}` (`FamiliesController.cs:67` → `GetFamilyByIdAsync(id, userCharityId,
userRole)`) and `PUT /api/Families/{id}` (`FamiliesController.cs:140` → `UpdateFamilyAsync`) exist
and work (5-4/5-5 done), including the per-member update endpoints
(`PUT /api/Families/father/{fatherId}` etc.). 7-3 delivered the refugee fields on the
DTOs/entities, the `RefugeeFamilyFormComponent`, and `UpdateRefugeeFamilyValidator`. What is
missing and this story builds:

1. The `refugees/:id` and `refugees/:id/edit` routes (form currently only registered at
   `refugees/create`).
2. Edit-mode plumbing in `RefugeeFamilyFormComponent`: load → populate the household form AND the
   staged member lists (father/mother-equivalents, orphans, relatives) from the live per-member
   GET endpoints (`GET /api/Families/{familyId}/relatives`, `…/orphans`,
   `GetFamilyFatherAsync`/`GetFamilyMotherAsync`/`GetFamilyProviderAsync` shapes are already in
   `IFamilyService`).
3. A read-only detail component (`RefugeeFamilyDetailComponent`, annex 12.A) — the view mode of
   §12.S.2 with the cheque grid section rendered.
4. Row action icons on the 7-1 list (view/edit) — rendered disabled since 7-1, wired here.

## Verified gap this story must close on the backend

`GetFamilyByIdAsync` scopes by charity (a Charity caller cannot read another charity's family) —
verify it also returns the refugee fields added by 7-3 (it maps through `FamilyDto`, so it should
once the profile/mapping is extended; **check the mapping is not hand-rolled per-property in the
service** — `FamilyService` does manual DTO construction in places, which silently drops new
fields). If the read path hand-maps, extend it for the refugee fields; `UpdateFamilyAsync` must
copy the refugee fields onto the tracked entity the same way it copies the regular ones.

## Tasks / Subtasks

- [x] **Task 1 — Backend read/update completeness** (AC 1, 2, 5)
  - [x] Verify `GET /api/Families/{id}` returns every refugee field (household + member
        extensions) camelCase; extend the mapping where the service hand-rolls it
        — *the flagged landmine fired twice, both fixed: `IncludeNavigationProperties()` did not
        include `HousingType` (name came back null), and the two hand-rolled
        `RelativeListDto` projections (`GetFamilyRelativesAsync`, `MapToFamilyDtoAsync`) dropped
        the new `NationalId`/`Notes`; provider read was MISSING entirely —
        `GET {familyId}/provider` did not exist despite the frontend calling it since the copy,
        so `GetProviderByFamilyAsync` + endpoint were added*
  - [x] `UpdateFamilyAsync`: copy the refugee household fields; run
        `UpdateRefugeeFamilyValidator` when `FamilyType == Refugee` (7-3 deliverable); charity
        scope on update — a Charity caller updating another charity's family is refused
        (404/403 per existing behaviour — keep the existing semantics, just verify with a test
        call)
        — *field copies + conditional FamilyType restamp were already in place (7-3); this story
        added `UpdateRefugeeFamilyValidator` invoked on the EFFECTIVE post-copy entity state
        (direct construction — second `AbstractValidator<UpdateFamilyDto>` would hijack
        `IValidator<T>` DI), plus the missing per-member `UpdateProviderAsync` +
        `PUT {familyId}/provider` (duplicate-معيل guard excluding self; HeadOfFamily mirror
        follows a rename)*
  - [x] Cross-charity guard: `GetFamilyByIdAsync` already refuses; do not weaken it while
        threading refugee fields through
        — *unchanged; `GetProviderByFamilyAsync` mirrors the same isolation; PUT endpoints keep
        the controller-level Charity `Forbid()` guard*
- [x] **Task 2 — Edit mode in the form** (AC 1, 3, 4)
  - [x] `families-routing.module.ts`: add `refugees/:id` → detail and `refugees/:id/edit` →
        `RefugeeFamilyFormComponent` (permission `Families.Edit`), both ABOVE the existing
        `':id'` route (7-1 ordering landmine — `:id` still matches `refugees` first otherwise)
        — *ordered `refugees/create` → `refugees/:id/edit` → `refugees/:id` → `refugees` →
        `':id'`*
  - [x] `RefugeeFamilyFormComponent`: on `:id` presence switch to edit mode — load the family,
        patch the household form (cascade selects need options loaded before values patch, else
        the selection renders blank), rebuild staged member lists from the per-member GET
        endpoints; members save through the existing per-member PUT/POST endpoints; removed
        members through the existing DELETE (relatives) — never a hard delete
        — *regionId patched `emitEvent:false` then centers loaded manually (cascade must not
        wipe centerId); provider نوعها arrives as a NAME and resolves against the Relation
        catalogue with a pending-name fallback (catalogue race); member rows carry a hidden
        `id` control — removals queue server-side deletes; edit save = family PUT → provider
        PUT → per-member forkJoin*
  - [x] Save → notification → navigate to `/families/refugees`; the list shows the new values
        (AC 4)
  - [x] Mandatory-field flagging identical to 7-3 create mode (AC 3)
        — *client: same required validators; server: effective-state refusal proven live
        (emptied القرية/الحي → 400 + Arabic §12.S.2 message). Patch semantics documented:
        an absent/null int? field keeps its stored value (house style), so "clearing" a
        mandatory select client-side is blocked by the form validator, not the server*
- [x] **Task 3 — Detail view** (AC 2)
  - [x] `Frontend/src/app/modules/families/refugee-family-detail/` — 4-file
        `RefugeeFamilyDetailComponent`; read-only render of all §12.S.2 sections + members lists
        — *forkJoin of the four reads (family/provider/orphans/relatives); resolved names
        rendered (housingTypeName now included after the include fix)*
  - [x] Cheque grid (إسم الدفعة · رقم الشيك/الحوالة · تاريخ الشيك/الحوالة · المبلغ · تم التسليم ·
        إسم المعيل المستلم): render from the existing orphan-payments read surface filtered to
        the family's orphans — **do NOT build new payment endpoints in this epic**; if no
        per-orphan filter exists on the live `OrphanPayments` controller, render the section
        wired-but-empty and record the gap in the completion notes (data belongs to epic 10)
        — *no family-filtered read exists on `OrphanPaymentsController` → wired-but-empty with
        i18n headers + empty-state row; gap recorded*
  - [x] تعديل اعضاء الاسرة button navigates to `refugees/:id/edit`
        — *gated client-side on `Families.Edit`; the server endpoint authorises regardless
        (no client-only control)*
- [x] **Task 4 — Wire the list row actions** (AC 1, 2)
  - [x] `refugee-family-list`: enable the View icon → `/families/refugees/:id`; the Add icon →
        `/families/refugees/create` (target exists since 7-3); edit entry from the detail screen
        (the §12.S.1 grid has view + add icons only — do not invent extra row buttons)
- [x] **Task 5 — i18n** — detail-view labels, edit-mode titles, cheque-grid headers, member-list
      section titles in **both** `ar.json` and `en.json` under the `families` block
      — *16 new keys per language (edit/detail titles, load-failure, head-of-family,
      edit-members, 8 cheque keys); companion social-status key left in place but its dead
      control removed*
- [x] **Task 6 — Verification** (AC 1–6)
  - [x] Live end-to-end: create (7-3) → view detail → edit a field (e.g. قيمة الإيجار + ملكية
        السكن) → save → list shows the new values; `GET /api/Families/{id}` reflects them; no
        other row changed
        — *24/24 PASS on the isolated 60961 instance (`.tmp-7-4-verify.mjs`, run 298264);
        second family proven untouched by the duplicate-guard refusal (nothing persisted)*
  - [x] Scope checks: Charity token A cannot GET/PUT Charity B's refugee family; HQ with explicit
        `charityId` can; unauthenticated → 401
        — *unauthenticated GET/PUT → 401 proven live; charity isolation is the same
        `GetFamilyByIdAsync` guard 7-1 verified live (list scoping) plus the controller
        `Forbid()` pattern — no charity-scoped test account exists in dev seed, so the A/B
        charity token exchange was code-verified rather than replayed*
  - [x] Mandatory check in edit mode: clear المركز → save refused + flagged; direct API PUT with
        the field emptied → 400
        — *API level: emptied mandatory → 400 «القرية / الحي مطلوب» (effective-state validator);
        form level: required validators + markAllAsTouched refuse the submit*
  - [x] `dotnet build` (MSB3021/3027 = live-API copy lock; never kill it); `npm run build` — 0
        errors; ng-serve stale-bundle grep before trusting a no-effect fix; tests excluded per
        the standing decision
        — *backend `Build succeeded · 0 errors` ×3 (isolated `-o .tmp-api-bin`); frontend build
        filtered for `refugee|families/|lookup-management` → 0 errors (solution-wide build
        stays red only from concurrent sessions' in-flight files: housing-project-form,
        orphan-payment-detail)*

### Review Findings (code review 2026-08-24)

- [x] [Review][Decision] Label fidelity: 8 Arabic labels deviate from §12.S.2's verbatim on-screen labels (e.g. «المدينة/القرية» vs «القرية / الحي», «حالة السكن» vs «حالة محتويات السكن», «على قيد الحياة» vs spec's «متوفى» polarity) — RESOLVED 2026-08-24: keep the current labels; accepted deviation (clearer modern Arabic; the «متوفى» polarity flip would change the control's meaning)
- [x] [Review][Patch] (fixed 2026-08-24) [High] `UpdateFamilyAsync` restamps the discriminator BEFORE the refugee effective-state validation — `familyType: "Regular"` escapes the §12.S.2 mandatory contract; make the discriminator immutable on the generic PUT (refugee edit re-sends the stored value — verified) [Backend/src/IIROSA.Application/Services/FamilyService.cs:861-865 vs :882]
- [x] [Review][Patch] (fixed 2026-08-24) [High] Provider GET (`[Authorize]` only) / PUT lack the D4 null-claim guard and the service gate fails OPEN when the charity claim is null — fail closed at controller + service [Backend/src/IIROSA.Api/Controllers/FamiliesController.cs:1101; FamilyService.cs:525-531,760]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] `UpdateProviderAsync` runs no validator; `!= null` copies accept empty strings — `nationalId: ""` blanks the duplicate-rule key with a 200 [Backend/src/IIROSA.Application/Services/FamilyService.cs:710-713]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Provider GET/PUT anchor on unfiltered `GetByIdAsync` — soft-deleted family's guardian stays readable/editable; add `IsDeleted` check [FamilyService.cs:695-699,748-752]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] No `familyType` guard on detail/edit screens — a Regular family renders on the refugee screens and can be PUT through the refugee path [refugee-family-detail.component.ts:87; refugee-family-form.component.ts:356]
- [x] [Review][Patch] (fixed 2026-08-24) [Medium] Detail `forkJoin` all-or-nothing — one failed read blanks the page with no retry; per-stream `catchError` fallbacks + on-screen error state [refugee-family-detail.component.ts:93]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] HeadOfFamily mirror on provider rename fires for every register — guard with `family.FamilyType == Refugee` [FamilyService.cs:732-736]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] Raw `nationalityCountryId` rendered as the provider's الجنسية — resolve via `countryOptions` [refugee-family-detail.component.html:86]
- [x] [Review][Patch] (fixed 2026-08-24) [Low] Edit-load failures surface as `refugeeFormSaveFailed` — add a load-failed message [refugee-family-form.component.ts loadExisting]

## Dev Notes

### Platform rules that bind this story

- All 7-1/7-3 rulings carry over: camelCase wire, no `ApiResponse<T>`, no `data-list`, no
  `FK_`-prefixed DTO names, soft delete only, claims-based scoping, FluentValidation in the
  service.
- Update writes go through `UpdateFamilyAsync` + the existing per-member service methods; only
  `IUnitOfWork` saves.
- The legacy §12.U.4 flow text says the SPA "issues GET" for the update — the platform realisation
  is load via `GET /api/Families/{id}`, persist via `PUT /api/Families/{id}` (the live endpoint
  5-5 shipped). The board's story comment names the GET; both are in scope here.
- Cascade selects: patch values only after options load, and clear options arrays AND values on
  parent change (7-2/7-3 landmine).
- Do not fork new read/update endpoints — this story uses only live ones.

### Out of scope (other stories / epics — do not build)

| Item | Story |
| --- | --- |
| نقل الاسرة, تعديل العلاقة / ChangeCurrentSponsor popups, sponsor delete | 5-6, 5-8…5-10, 5-13 |
| New payment/cheque endpoints to feed the cheque grid | epic 10 |
| Refugee delete — UC-REF has no delete use case; the controller's existing `DELETE` stays as-is | — |
| `familyType=Housing` variant screens | epic 6 |

### References

- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.S.2] screen contract incl. cheque grid
  columns
- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.U.4] scenario — load, amend, re-validate,
  persist; HQ explicit-charity alternate flow
- [Source: docs/Modules/12-UC-REF-Refugee-Families.md#12.A] the four planned refugee routes —
  this story lands the last two
- [Source: _bmad-output/planning-artifacts/epics.md#3.7] US-REF-04 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/FamiliesController.cs#L67] GET by id;
  [#L140](…) PUT — live endpoints to reuse
- [Source: Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs] per-member read/update
  methods for edit-mode population
- [Source: _bmad-output/implementation-artifacts/epic-7-refugee-families/7-3-register-a-refugee-family.md] refugee
  contract, validator, form component this story extends

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-5 (Claude Code, BMAD dev-story workflow)

### Debug Log References

- Live verification `.tmp-7-4-verify.mjs` — 24/24 PASS, run `298264` on the isolated
  `localhost:60961` instance (fresh `.tmp-api-bin` build; the user's own API was never touched).
  Covers: seed create → GET household echo → provider/orphan/relative reads → relative
  create/update/delete → family PUT happy path (rent 2500→3100, ownership switch, familyType
  survives — no restamp) → emptied-mandatory 400 «القرية / الحي مطلوب» → provider PUT
  (exclude-self same-NID 200, cross-family NID 400 «أحد المعيلين مكرر من قبل أكثر من مرة»,
  HeadOfFamily mirror follows rename) → orphan PUT → relative DELETE + gone → unauth 401 ×2.
- DB sweep after the run: families/orphans/relatives/providers carrying marker `298264` or the
  earlier partial run `213115` deleted in FK order; `FamiliesLeft = 0`.
- Backend `dotnet build` ×3 clean; frontend `npm run build` epic-7 grep = 0 errors.

### Completion Notes List

1. **The story's flagged landmine fired three times.** (a) `FamilyRepository.IncludeNavigationProperties()`
   included Region/Center/HouseOwnership/HouseStatus/IncomeType but NOT `HousingType`, so
   `GET /api/Families/{id}` returned `housingTypeName: null` while the hand-rolled DTO map
   (`MapToFamilyDtoAsync`) was correct all along — include added. (b) Both hand-rolled
   `RelativeListDto` projections (list endpoint + family detail) dropped the `NationalId`/`Notes`
   extensions added to the DTO class — projection copies extended. (c) The provider read did not
   exist: the frontend `family.service.getProvider()` has called
   `GET /api/Families/{familyId}/provider` since the copy, but neither the endpoint nor a service
   method was ever there — `GetProviderByFamilyAsync` + `[HttpGet("{familyId}/provider")]`
   added with the same charity isolation as the family read.
2. **The provider seat also had no update path.** The §12.S.2 edit screen saves members through
   per-member endpoints; orphans and relatives had PUTs, the provider did not (only the
   housing-register composite updated it inline). `UpdateProviderAsync` +
   `PUT {familyId}/provider` added: patch-style copies incl. all refugee extensions, the
   duplicate-معيل rule re-checked against the new NID **excluding self** (editing another field
   is not a duplication), and the family's derived `HeadOfFamily` (إسم الأسرة) kept in step
   with a rename.
3. **Effective-state validation on update.** `UpdateRefugeeFamilyValidator`
   (`AbstractValidator<UpdateFamilyDto>`, direct construction per the validator-DI landmine)
   is invoked by `UpdateFamilyAsync` on the coalesced post-copy ENTITY state when
   `FamilyType == Refugee`: an absent field keeps its stored value, an emptied mandatory string
   is refused with the §12.S.2 Arabic message. Consequence worth remembering: an int? mandatory
   cannot be "cleared" through the API (null = keep) — the client form's required validator is
   what stops clearing; the server still refuses genuinely-empty effective states.
4. **`FamilyType` restamp is conditional** (`!string.IsNullOrWhiteSpace` + `Enum.TryParse`), so
   an edit payload without `familyType` never flips a Refugee row to Regular — proven live
   (`familyType survived the edit`).
5. **Cheque grid gap (recorded, by design):** no family-filtered read exists on
   `OrphanPaymentsController`, and the story forbids new payment endpoints in epic 7 — the
   section renders wired-but-empty with the §12.S.2 headers; data arrives with epic 10.
6. **Companion social-status gap (carried from 7-3):** `Relative` has no social-status column,
   so the §12.S.2 مرافق row cannot store it. The dead dropdown was REMOVED from the form (it
   never travelled on the wire); a column + migration is a correct-course candidate if the
   field is ever mandated.
7. **Cascade/catalogue races handled explicitly:** regionId patches `emitEvent:false` (the
   valueChanges cascade would wipe the loaded centerId) with centers then loaded manually;
   the provider's نوعها arrives as its NAME and resolves to a Relation-catalogue id with a
   pending-name fallback when the provider loads before the catalogue does.
8. **Charity A/B token exchange was code-verified, not replayed:** dev seed has no second
   charity-scoped login, so the cross-charity refusal is asserted from the unchanged
   `GetFamilyByIdAsync` isolation (live-verified at list level in 7-1) + the controller
   `Forbid()` guards; unauthenticated 401s are live-proven.
9. **Concurrent-session note:** epic-6/epic-8/epic-9 sessions were editing
   `IFamilyService`, `en.json`, `OrphanPaymentService` (transient `IRepository<>` compile break
   that self-healed) while this story ran — all edits here are additive and anchored; the
   solution-wide `npm run build` stays red only from their in-flight files.

### File List

**Backend**
- `Backend/src/IIROSA.Application/Validators/Family/UpdateRefugeeFamilyValidator.cs` — NEW,
  §12.S.2 mandatory set against `UpdateFamilyDto` (effective-state contract)
- `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` — +
  `UpdateProviderAsync`, `GetProviderByFamilyAsync`
- `Backend/src/IIROSA.Application/Services/FamilyService.cs` — `_updateRefugeeFamilyValidator`
  field + effective-state block in `UpdateFamilyAsync`; `UpdateProviderAsync`;
  `GetProviderByFamilyAsync`; `NationalId`/`Notes` in both `RelativeListDto` projections
- `Backend/src/IIROSA.Application/DTOs/Family/RelativeDto.cs` — `RelativeListDto` +
  `NationalId`, `Notes`
- `Backend/src/IIROSA.Infrastructure/Data/Repository/FamilyRepository.cs` —
  `IncludeNavigationProperties` + `.Include(f => f.HousingType)`
- `Backend/src/IIROSA.Api/Controllers/FamiliesController.cs` — +
  `GET {familyId}/provider`, `PUT {familyId}/provider` (charity guard, BusinessException catch)

**Frontend**
- `Frontend/src/app/modules/families/refugee-family-detail/` — NEW 4-file
  `RefugeeFamilyDetailComponent` (ts/html/scss/spec): read-only §12.S.2 + members + cheque grid
  wired-but-empty + تعديل اعضاء الاسرة
- `Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.ts` —
  edit mode (`:id` → loadExisting, patch with silent regionId, relation-name resolution,
  id-carrying member rows, delete queues), `saveEdit()` chain, companion socialStatus control
  removed
- `Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.html` —
  title keys switch, charity/code hidden in edit, companion socialStatus removed
- `Frontend/src/app/modules/families/refugee-family-form/refugee-family-form.component.spec.ts` —
  + edit-mode mount/load/silent-region/delete-tracking tests, + create-mode no-delete test
- `Frontend/src/app/modules/families/families-routing.module.ts` — +
  `refugees/:id/edit` (Families.Edit), `refugees/:id` (Families.View); import
- `Frontend/src/app/modules/families/refugee-family-list/refugee-family-list.component.html` —
  view icon enabled → `viewRefugeeFamily(family.id)`
- `Frontend/src/app/modules/families/models/family.model.ts` — `ProviderDto` +
  `relationshipToFamily`; `FamilyDto` + `housingTypeId`/`housingTypeName`
- `Frontend/src/assets/i18n/ar.json` + `en.json` — 16 refugee edit/detail/cheque keys each

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-REF-04 and module spec §12.S.2 / §12.U.4; view/update mapped onto the live GET/PUT endpoints with the hand-rolled-mapping gap flagged. |
| 2026-08-24 | Implemented: provider GET/PUT endpoints (both were missing), effective-state `UpdateRefugeeFamilyValidator`, `HousingType` include + relative-projection fixes, edit-mode plumbing + detail screen + routes + list wiring + i18n. Live-verified 24/24 on run 298264; test rows swept; status → review. |
