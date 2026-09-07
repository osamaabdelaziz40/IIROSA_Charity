# Story 16-8: Select the routing department

| Field | Value |
| --- | --- |
| Story key | `16-8-select-the-routing-department` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-08 — الإدارة المختصة |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.U.8 scenario) |
| Route | consumed by the incoming/outgoing forms + list filters (`#/incoming-outgoing/…`) |
| Endpoint | `GET /api/LookupManagement/departments` |
| Depends on | EP-03 lookups module (endpoint already live); 16-4/16-13 render the dropdowns |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to select the routing department الإدارة
المختصة, so that letters are routed to the office that handles them.

## Acceptance Criteria

1. Given a head-office staff member with an active session in the module, when the actor opens the
   screen with valid input, then no stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/LookupManagement/departments` and the response is rendered on the screen without a
   page reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the scenario of §21.U.8 passes end to end; the department catalogue
serves the dropdowns of both letter forms and both list filters; scoping is enforced
server-side.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

| Layer | File | State |
| --- | --- | --- |
| Lookup | `Department` (`LookupEntity`, int) + `DepartmentConfiguration` + `LookupManagementController` `GET departments` (`:467`) | Exists and registered |
| Frontend | both letter forms already call `lookupService.getDepartments({ isActive: true })` and read `response.items` (`LookupPagedResult` — correct) | Exists |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Empty catalogue.** `SeedData` seeds roles/users/mission lookups/technical support only —
   **no Department rows exist**, so every dropdown renders empty on a fresh database. Seed the
   WAR-consistent HQ departments (Arabic-first, `NameAr` populated, `NameEn` mirror),
   existence-guarded like the 15-1 mission-lookup seed (skip rows that already exist).
2. **The list screens never load departments.** `selectedDepartment` exists in both list form
   models but no template control renders it and no lookup call loads it (16-1 defect 5) — the
   list filter dropdowns land here (bound to the fixed filter DTO key).
3. **`loadingLookups` never clears on success** in both forms (`incoming-letter-form.component.ts:161-186`) — the flag is set, then only cleared in the error branch; set it false in both
   branches.
4. **Dropdown display value.** Options must label with `NameAr ?? NameEn` (bilingual rule) and
   bind the int `Id`; the forms' transform does this for outgoing letters' options — apply the
   same shape for departments consistently.

## Tasks / Subtasks

- [x] **Task 1 — Seed the HQ department catalogue** (AC 2)
  - [x] `SeedData`: Arabic-first `Department` rows (e.g. الإدارة العامة، الشؤون المالية،
        شؤون الأيتام، المراسلات، تقنية المعلومات — align final names with the WAR term list),
        `IsActive = true`, guarded against existing rows; runs with the app's seed path
- [x] **Task 2 — Serve every department consumer** (AC 1)
  - [x] Verify `GET /api/LookupManagement/departments` returns `LookupPagedResult` with
        `NameAr/NameEn/IsActive`; the forms keep working; both **list filters** gain the الادارة
        drop-down bound to `departmentId` (16-2 wire)
  - [x] Fix `loadingLookups` clearing; label via `NameAr ?? NameEn`
- [x] **Task 3 — Scoping + auth** (AC 3, 4, 5)
  - [x] Departments are HQ-catalogue lookups (not charity-owned rows) — confirm the endpoint's
        authorization shape (lookup module's existing `[Authorize]`) and that no charity user can
        mutate them through this module; read access is module-wide by design — record that
        decision in the story notes
- [x] **Task 4 — Verification**
  - [ ] Live: departments endpoint returns seeded rows; form + filter dropdowns populate; both
        i18n files gain any new label keys
  - [x] `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

- Lookup entities are `LookupEntity` (int key) in `MappingDefaults.LOOKUP_SCHEMA`; never invent a
  new schema string.
- Seeds are existence-guarded and Arabic-first (`NameAr` mandatory, `NameEn` mirror).
- This is the only story that touches the lookup side — do not refactor the lookup module itself.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Outgoing category catalogue (sibling lookup) | 16-17 |
| Employees lookup wiring | 16-4, 16-9 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.8] scenario
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.2] الادارة او
  الجهة الراسله للخطاب — mandatory, Departments lookup
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-08 acceptance criteria
- [Source: Backend/src/IIROSA.Api/Controllers/LookupManagementController.cs#L467] live endpoint
- [Source: Backend/src/IIROSA.Infrastructure/Data/SeedData/] no Department seed today

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): verified `CorrespondenceLookupSeedData.SeedDepartmentsAsync` — six Arabic-first rows (رئاسة المكتب، رعاية الأيتام، المشاريع، الشؤون المالية، شؤون الجمعيات، الشؤون الإدارية) with `NameEn` mirrors + `IsActive` + `SortOrder`, existence-guarded (`AnyAsync` skip), wired via `IIROSASeedDataInitializer.SeedAllAsync` (:79). `loadingLookups` no longer exists in either form — the previous session's form rebuild removed the flag outright, closing defect 3 by removal.
- 2026-08-24 (verification): both list filters gained the الادارة dropdown this pass (incoming + outgoing list components, bound to `departmentId` on the 16-2 wire); forms keep using `lookupService.getDepartments({ isActive: true })` with `NameAr ?? NameEn` labelling. Project builds clean; `ng build` clean for the module.

### Completion Notes List

- Catalogue rows chosen to align with the WAR HQ department list (final names recorded above — spec's own examples were generic); adjust by editing the seed on a fresh database (existence guard keeps live rows untouched).
- Scoping decision recorded (Task 3): departments are HQ-catalogue lookups (`LookupEntity`, LOOKUP_SCHEMA), **not charity-owned** — read access is module-wide by design through the lookup module's existing authorization; mutation stays in the lookup module (out of this epic's scope), so no charity user can mutate them through correspondence.
- Task 4 live check left unchecked — pending the user's `IIROSA.Api` restart (the seed runs on startup). Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Infrastructure/Data/SeedData/CorrespondenceLookupSeedData.cs` — department seed (+ outgoing categories, 16-17)
- `Backend/src/IIROSA.Infrastructure/Data/SeedData/IIROSASeedDataInitializer.cs` — seed wiring
- `Frontend/src/app/modules/incoming-outgoing/incoming-letters/incoming-letters-list.component.ts` / `.html` — الادارة filter dropdown
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letters-list.component.ts` / `.html` — الادارة filter dropdown

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-08 and module spec §21.U.8; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (verified seed + wiring; both list filters gained the department dropdown this pass). Status → review; live seed check pending user's API restart. |
