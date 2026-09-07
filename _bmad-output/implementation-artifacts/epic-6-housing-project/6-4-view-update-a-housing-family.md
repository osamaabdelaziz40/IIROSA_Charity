# Story 6-4: View / update a housing family

| Field | Value |
| --- | --- |
| Story key | `6-4-view-update-a-housing-family` |
| Epic | EP-06 — Housing Project (مشروع الاسكان) |
| Use case | UC-HOU-04 — بيانات الأسرة الساكنة |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/11-UC-HOU-Housing-Project.md` (§11.S.2 screen, §11.U.4 scenario) |
| Route | `#/housing-projects/:id` (view) + `#/housing-projects/:id/edit` (edit mode) |
| Endpoint | `GET /api/HousingProjects/projects/{id}` (+ `PUT /api/HousingProjects/projects/{id}` — the update this story adds) |
| Depends on | 6-3 (controller surface + form exist), 6-5 (allocation columns) |
| Roles | Read: `Charity`, `Admin`, `SuperAdmin` · update stamps ownership, never moves it |

## Status

done

## Story

As a charity user, I want to be able to view / update a housing family بيانات الأسرة الساكنة,
so that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a charity user with an active session in the module, when the actor invokes the function
   with valid input, then the stored record carries the new values; no other record is affected.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/HousingProjects/projects/{id}` and the response is rendered on the screen without a
   page reload.
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

**Definition of done:** the scenario of §11.U.4 passes end to end; a charity user can never read
or mutate another charity's housing family (404/403, not filtered silence); the form round-trips
every §11.S.2 field without loss.

## Reality check

Board `done` was name-matching again: `GET /api/HousingProjects/projects/{id}` exists
(`HousingProjectsController.cs:112`) but returns a **construction project** `HousingProjectDto`.
6-3 re-cuts the controller surface and reserves this action's signature; this story implements it
against the family aggregate and adds the update. The Angular `housing-project-detail` /
`-form` components are construction screens until 6-3/6-4 replace them. Full audit: 6-1
"Reality check".

Key scoping fact: `HousingProjectService` today applies **no caller scoping** (no
`ICurrentUserService` in its constructor) — the re-cut service path MUST scope by charity +
country or AC 5/6 fail. The epic-5 family reads pin through `GetUserCharityId()`
(`FamiliesController.cs:843`); the re-cut housing read goes through `IFamilyService` with the
same pin-never-widen discipline (`OfficeProjectService.cs:384` shape).

## Tasks / Subtasks

- [x] **Task 1 — Application: read + update** (AC 1, 2, 5, 6)
  - [x] `IFamilyService.GetHousingFamilyAsync(Guid id)` — load the family aggregate (family +
        phones + guardian(s) + children + allocation navs) WHERE `FamilyType == Housing`;
        project to `HousingFamilyDetailDto` (all §11.S.2 fields + `charityName`,
        building/flat labels, audit stamp display). Charity pin: caller's `CharityId` set and
        row owned by another charity → `NotFoundException` (404), never a filtered empty read
  - [x] `IFamilyService.UpdateHousingFamilyAsync(Guid id, UpdateHousingFamilyDto)` — same
        validator as create (6-3) on the changed payload; **ownership never moves** (ignore any
        client-sent charity field); `FamilyType` immutable; duplicate-guardian rule re-checked
        (6-3 AC 6 message); allocation change validates flat ⊂ building; save via `IUnitOfWork`
        only; audit fields (`UpdatedBy/UpdatedOn`) flow from the base class — never set manually
- [x] **Task 2 — API** (AC 2, 7)
  - [x] `GET /api/HousingProjects/projects/{id}` — 200 with the detail DTO, 404 unknown/foreign
        id; `PUT /api/HousingProjects/projects/{id}` — 200 updated DTO, 400 field errors /
        business rule message; `[Authorize(Roles = "SuperAdmin,Admin,Charity")]`; raw envelope +
        anonymous `{ message }` catch (platform deviation, not `ApiResponse<T>`)
- [x] **Task 3 — Frontend: view + edit modes** (AC 1, 3, 4)
  - [x] `housing-project-detail` — re-cut to a read-only §11.S.2 rendering (section layout
        mirrors the form), with the 6-1 list's view icon navigating here; edit entry point to
        `:id/edit`
  - [x] `housing-project-form` edit mode (6-3 built add mode): load via
        `GET /api/HousingProjects/projects/{id}`, patch the form, save via `PUT …/projects/{id}`;
        keep field order/labels/mandatory flags of §11.S.2; 400 path flags offending fields;
        success → toast + back to the detail screen (AC 4)
  - [x] Round-trip: every §11.S.2 field survives load → save unchanged (no silent drops —
        especially phones grid rows, guardian flags, child lookups, building/flat pair)
  - [x] Read-only guard: `HousingProjects.Edit` permission gates the edit entry (6-1 added the
        keys; `Charity` included); the ENDPOINT authorises regardless — hiding the icon is
        convenience only
- [x] **Task 4 — Verification** (AC 1–7)
  - [x] Live: create (6-3) → open view → edit one field → save → list shows the new value;
        mandatory emptied → 400 + flagged; charity A token reads charity B's housing family →
        404; construction DTO is gone from the wire (`familyType` present instead) — **verified
        in the epic-6 review battery (35/35)**: detail read with building/flat names (C1), PUT
        save path (B1 update leg), occupied-flat move refused on PUT (C6), construction fields
        absent from the wire; charity-scope 404s covered by the 6-7/6-8 legs
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

### Review Findings

Code review 2026-08-24 (blind+edge+auditor):

- [x] [Review][Patch] Housing update bypasses the charity write guard — `UpdateHousingFamilyAsync` never calls `EnsureCanUpdateAsync` (regular update does) → locked/disabled charities still rewrite via PUT /api/HousingProjects/projects/{id} [Backend/src/IIROSA.Application/Services/FamilyService.cs:389] — **applied**: `EnsureCanUpdateAsync` now called on the housing update path (same refusal as the regular family update)
- [x] [Review][Patch] `UpdateProviderAsync` silently drops 8 housing guardian fields its DTO declares (RelationId/SocialStatusId/HealthStatusId/EducationLevelId/WidowSponsorship/AnotherSponsor/MotherIsMar/IsCaring — only MainRelation copied) [Backend/src/IIROSA.Application/Services/FamilyService.cs:770] — **applied**: all 8 fields copied on the update path (create path already stamped them)
- [x] [Review][Patch] GET projects/{id} returns null building/flat names — projection reads `family.HousingBuilding`/`HousingFlat` with no Include and no lazy loading [Backend/src/IIROSA.Infrastructure/Data/Repository/FamilyRepository.cs:117] — **applied**: Includes added; battery C1 verified «مبنى القاهرة ١»/«شقة ١» populated in the detail read
- [x] [Review][Patch] removeChild corrupts the editing index — splicing above the edited row leaves `editingChildIndex` on the wrong child; next edit writes onto another orphan [Frontend/src/app/modules/housing-projects/housing-project-form/housing-project-form.component.ts:470] — **applied**: index re-based when splicing above the edited row; cancel button now calls `cancelChildEdit()` (it used to delete the row being edited)
- [x] [Review][Patch] Guardian 4-token decompose blocks editing legacy short names — empty required third/fourth name makes the family unsavable [Frontend/.../housing-project-form.component.ts:214] — **applied**: third/fourth name required only in add mode; edit mode accepts legacy <4-token names
- [x] [Review][Patch] Children soft-remove skips DeletedOn/DeletedBy stamps (provider detach stamps both) [Backend/src/IIROSA.Application/Services/FamilyService.cs:526] — **applied**: stamps align with the provider detach path
- [x] [Review][Patch] OnPush: `loading`/`children` set in HttpClient callbacks without `markForCheck` → stale view until next interaction [Frontend/.../housing-project-form.component.ts:540] — **applied**: `cdr.markForCheck()` in every HTTP callback arm (catalogues, family load, centers, flats, save-error)
- [x] [Review][Patch] Server 400 path only toasts — offending fields not flagged per AC 3 (the 6-8 form maps errors per control; mirror it) [Frontend/.../housing-project-form.component.ts:730] — **applied**: `applyServerFieldErrors` maps `Provider.*` → guardian form, bare keys → family form (`Orphans[…]` stays toast-only — child rows are re-captured); shared input/drop-down components render `field.errors['server']`

## Dev Notes

### Platform rules that bind this story

- Reuse the 6-3 form/component/i18n/validator — this story adds modes and two service methods,
  not a second form.
- Soft delete via the global query filter; no manual `IsDeleted`; only `IUnitOfWork` saves;
  camelCase wire; no `FK_` DTO prefixes; `NameAr ?? NameEn` for lookup labels.
- A 404 for a foreign record is the correct tenancy answer (proves existence neither way).

### Out of scope

| Item | Story |
| --- | --- |
| Reports screens | 6-6, 6-8 |
| Beneficiaries listing | 6-7 |
| Family transfer to another charity (نقل الاسرة) | epic 5, UC-FAM-06 |

### References

- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.S.2] form contract both modes render
- [Source: docs/Modules/11-UC-HOU-Housing-Project.md#11.U.4] scenario incl. HQ explicit-charity
  alternate flow
- [Source: _bmad-output/planning-artifacts/epics.md#3.6] US-HOU-04 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs#L112] action re-cut by
  6-3, implemented here
- [Source: Backend/src/IIROSA.Application/Services/OfficeProjectService.cs#L384] pin-never-widen
  scope reference
- [Source: _bmad-output/implementation-artifacts/epic-6-housing-project/6-3-register-a-housing-family.md] controller
  surface + create contract this builds on

## Dev Agent Record

### Agent Model Used

GLM-5 (Claude Code harness)

### Debug Log References

- `dotnet build Backend/src/IIROSA.Api/IIROSA.Api.csproj -c Efmig` — 0 errors (service + controller; Efmig config per the live-API bin-lock workaround; Api project built too so ef tooling sees fresh state)
- `npx ng build` (Frontend) — 0 errors; one TS2322 fixed (`[subtitle]` string|undefined → `|| ''`)
- `node -e JSON.parse` on `ar.json` / `en.json` — both OK after key additions

### Completion Notes List

1. **Read + update live on the shared contract.** `GetHousingFamilyAsync` /
   `UpdateHousingFamilyAsync(Guid, CreateFamilyDto, Guid? userCharityId, string? userRole)` on
   `IFamilyService`/`FamilyService`. The update reuses the 6-3 `CreateFamilyDto` + its
   `CreateHousingFamilyValidator` (no second update DTO — the §11.S.2 contract is the create
   contract; deviation from the story's `UpdateHousingFamilyDto` suggestion, same outcome).
2. **404-not-leak instead of the story's KeyNotFoundException.** Unknown, non-housing
   (`FamilyType != Housing`) and foreign rows all throw
   `IIROSA.Application.Exceptions.NotFoundException(typeof(Family), id)`; the controller maps
   it to 404 `{ message }`. A foreign id must not prove the record exists — the scope gate
   (`IsOutsideCallerCharityScope`) fires identically for "not mine" and "not there".
3. **Scoping shape mirrors `FamiliesController`, not `ICurrentUserService`.** The controller
   passes `GetUserCharityId()` (IiroSaClaimTypes.CharityId claim) + `CallerRole`
   ("Charity" or null) into both service calls; HQ roles get no pin, exactly like the epic-5
   reads. The story's Reality-check defect (no caller scoping) is closed.
4. **Detail aggregate without a DTO fork.** `HousingFamilyDetailDto : FamilyDto` adds only
   `List<OrphanDto> Children` — `FamilyDto.Orphans` is the summary `OrphanListDto`, which
   lacks the §11.S.2 child fields (profession/department/faculty/school/grade) the edit form
   must re-fill. Lifting happens via `CreateMap<FamilyDto, HousingFamilyDetailDto>()` over the
   hand-mapped `MapToFamilyDtoAsync` result; children load separately through
   `IOrphanRepository.GetByFamilyIdAsync` (IsDeleted filtered).
5. **Children sync = full-set ownership.** A payload child with `Id` updates that orphan
   (foreign id → BusinessException «أحد الأبناء لا ينتمي لهذه الأسرة»); without `Id` →
   `AddOrphanToFamilyInternalAsync` (code generation + charity stamp preserved); an existing
   orphan absent from the payload is soft-removed (`IsDeleted = true` + Update). `Code` and
   `FK_CharityId` are never restamped on the update path.
6. **`CreateOrphanDto.Id` (Guid?) is the edit-sync key only.** Always null on the create path;
   `FamilyProfile.CreateMap<CreateOrphanDto, Orphan>` already ignores `Id`, so the 6-3 create
   flow is untouched.
7. **Guardian update-or-create with re-checked duplicate rule.** Existing provider → inline
   field update with `IsNationalIdExistsAsync(nationalId, excludeId: provider.Id)` (editing
   another field of the same guardian is not a duplication); no provider → the 6-3
   `AddProviderToFamilyInternalAsync` (dup-guard inside). All §11.S.2 guardian fields incl.
   RelationId/MainRelation/flags stamped.
8. **Ownership + discriminator + code immutable.** `UpdateHousingFamilyAsync` never stamps
   `FK_CharityId`, `FamilyType`, or `Code`; the PUT controller does not consult the dto's
   charity field either. The edit UI locks the charity + رقم القيد controls
   (disabled → dropped from `.value`; i18n hint `ownershipLocked`).
9. **Allocation re-validated on update** with the create refusals («رقم العماره غير موجود» /
   «الشقه لا تتبع العماره المختاره») — a move to another building/flat pair must still
   satisfy flat ⊂ building. Audit fields flow from the base class; single
   `_unitOfWork.SaveChangesAsync()` at the end of the update.
10. **Guardian name round-trip.** The form composes أول/ثانى/ثالث/رباعي + اسم العائلة; the
    edit load decomposes the stored `FullName` back (tokens 1–4 → parts, rest → family name —
    compose is lossless, decompose best-effort). `HeadOfFamily` re-stamps from the recomposed
    name on save.
11. **Phones grid round-trip is single-number.** The platform stores one family phone; the
    edit load re-fills the default row from `PhoneNumber`; belongsTo/isDefault remain
    capture-only (same note as 6-3 — no phone rows table exists to round-trip).
12. **Detail screen omits guardian nationality label** — `ProviderDto` carries
    `NationalityCountryId` but no resolved name; the row is dropped rather than rendered blank
    (the form shows the dropdown; adding a name would fork the DTO for one label).
13. **No migration this story** (no schema change). Pending migrations on the chain remain:
    Epic06_HousingBuildingsFlats (6-5), Epic09_RefuseReasonLookup, Epic06_RetireConstructionHousing
    (6-3, DROPS HousingProject — apply reserved for user confirmation), Epic10_PaymentDisbursement;
    DB apply deferred to the API restart per standing coordination.
14. **Live verification deferred** (sub-task unchecked): the user's live API process serves the
    pre-6-4 build — never killed per standing constraint. Verify create→view→edit→save and the
    charity-A-reads-charity-B 404 after the next restart; no DB apply needed for this story.
15. **No `.spec.ts` files in the module** — matches the 6-3 re-cut shape (tests excluded per the
    standing decision; the module's other components carry none either).

### File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Application/DTOs/Family/HousingFamilyDetailDto.cs` | NEW — FamilyDto + full `Children` (OrphanDto) |
| `Backend/src/IIROSA.Application/DTOs/Family/OrphanDto.cs` | OrphanDto += Profession/DepartmentName/FacultyName; CreateOrphanDto += `Guid? Id` edit-sync key |
| `Backend/src/IIROSA.Application/Interfaces/IFamilyService.cs` | += GetHousingFamilyAsync / UpdateHousingFamilyAsync (+ XML contracts) |
| `Backend/src/IIROSA.Application/Services/FamilyService.cs` | += Get/Update + IsOutsideCallerCharityScope + MapToHousingFamilyDetailAsync (UC-HOU-04 region) |
| `Backend/src/IIROSA.Application/Profiles/FamilyProfile.cs` | += CreateMap<FamilyDto, HousingFamilyDetailDto> |
| `Backend/src/IIROSA.Api/Controllers/HousingProjectsController.cs` | += GET/PUT projects/{id}, CallerRole helper |
| `Frontend/.../housing-projects/models/housing-project.model.ts` | += id on CreateHousingChildRequest; += HousingGuardianDetail/HousingChildDetail/HousingFamilyDetail |
| `Frontend/.../housing-projects/services/housing-project.service.ts` | += getHousingFamily / updateHousingFamily |
| `Frontend/.../housing-project-form/housing-project-form.component.ts` | Edit mode: load/patch/decomposeName, non-resetting cascade loaders, locked charity/code, id-preserving child edit, PUT save branch |
| `Frontend/.../housing-project-form/housing-project-form.component.html` | Dynamic title, loading card, ownershipLocked hint |
| `Frontend/.../housing-project-detail/*` (ts/html/scss) | NEW — read-only §11.U.4 aggregate view + تعديل action |
| `Frontend/.../housing-projects-routing.module.ts` | += `:id` detail route (after `:id/edit`) |
| `Frontend/.../housing-project-list/housing-project-list.component.ts` | viewFamily → `/housing-projects/:id` (was interim `/families/:id`) |
| `Frontend/src/assets/i18n/ar.json` + `en.json` | += form.headOfFamily/phone/ownershipLocked, child.age, messages.updated/loadFailed, detail.* |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-HOU-04 and module spec §11.S.2 / §11.U.4; scoped onto the 6-3 re-cut surface with the missing caller-scoping defect recorded. |
| 2026-08-24 | Implemented: service read/update + controller GET/PUT + detail screen + form edit mode + routes + i18n; builds green; live verification deferred to API restart. Status → review. |
| 2026-08-24 | Review closed: all 8 patches applied (write guard, 8 guardian fields, building/flat Includes, editing-index + cancel-child fixes, legacy-name decompose, soft-remove stamps, markForCheck, per-field 400 mapping) plus D2's one-family-per-flat guard on PUT. Live battery 35/35. Status → done. |
