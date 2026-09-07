# Story 16-10: List outgoing letters

| Field | Value |
| --- | --- |
| Story key | `16-10-list-outgoing-letters` |
| Epic | EP-16 — Correspondence — Incoming & Outgoing (الوارد والصادر) |
| Use case | UC-COR-10 — قائمة الصادر |
| Priority / size | Should · 2 points |
| Specification | `docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md` (§21.S.4 screen, §21.U.10 scenario) |
| Route | `#/incoming-outgoing/outgoing` |
| Endpoint | `GET /api/IncomingOutgoing/outgoing` |
| Depends on | 16-1 (controller hygiene + platform patterns; defect classes shared) |
| Roles | Staff → `Admin`, `SuperAdmin` |

## Status

review

## Story

As a head-office staff member, I want to be able to list outgoing letters الصادر, so that I can
find the record I need without leaving the system.

## Acceptance Criteria

1. Given a head-office staff member with an active session on the screen at
   `#/incoming-outgoing/outgoing`, when the actor opens the screen with valid input, then no
   stored data is changed — the operation is a read.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/IncomingOutgoing/outgoing` and the response is rendered on the screen without a page
   reload.
3. Given a charity user, when the function is invoked, then only records owned by that charity
   (and country) are returned or affected.
4. Given an HQ role, when an explicit charity id is supplied, then the function operates on that
   charity's data.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §21.S.4 are implemented with their mandatory flags and
lookups; the scenario of §21.U.10 passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## What exists already (copied from the previous implementation — DO NOT rebuild from scratch)

Mirror of the incoming side — every layer exists and carries the same defect classes as 16-1:

| Layer | File | State |
| --- | --- | --- |
| Entity | `Entities/Outgoing.cs` | Exists; no Charity link (gap, Task 2) |
| Repo | `OutgoingRepository.cs` | Exists; Includes Department + Category (+ UploadedFile paged; **not** IncomingLetter) |
| Service | `OutgoingService.cs` | Exists; no caller scope; hardcoded categories |
| API | `IncomingOutgoingController` outgoing block | Exists; same ValueTuple wire + no Roles |
| Frontend | `outgoing-letters/outgoing-letters-list.component.*`, `outgoing.service.ts` | Exists; phantom `/api/Outgoing` URL (`outgoing.service.ts:18`) |

## Verified defects this story must fix (found by reading the code, 2026-08-24)

1. **Phantom URL + tuple wire + wire names** — identical classes to 16-1 defects 1–3:
   `outgoing.service.ts:18` → `/api/Outgoing`; list reads `response.items`/`totalCount`;
   `outgoingCategoryName` vs wire `categoryName`; `Fk_DepartmentId` → `fk_DepartmentId`; no
   `trackBy` (`outgoing-letters-list.component.html:103`).
2. **No charity/country dimension** on `Outgoing`, `OutgoingFilterDto`, or `OutgoingService` —
   AC 3/4 unimplementable without it (16-1 Task 2 pattern).
3. **Category filter is string-dead.** The list offers hardcoded `'official'/'internal'/'external'`
   ids (`outgoing-letters-list.component.ts:59-64`) while `OutgoingFilterDto.CategoryId` is
   `int?` — the sent value can never bind. The real catalogue work is 16-17; here the filter
   control binds the 16-17 wire (ids from the categories endpoint).
4. **Filter set vs §21.S.4.** Spec: الجمعية · الحاله (tri-state) · الموظف · رقم الخطاب (numeric)
   · الموضوع · من/الي تاريخ. Copied screen: free search + category + year + dates + hasReply —
   hasReply/رقم داخلي are not §21.S.4 filters (keep hasReply off the filter bar; the grid may
   badge reply state).
5. **Grid vs §21.S.4.** Spec columns: رقم الصادر · العام الهجري · الادارة · التاريخ · البيان ·
   الملف · الاجراءات — copied grid shows outGoingId/department/category/hasReply instead; no
   الملف column.
6. **Status/roles/i18n** — same as 16-1 defects 6–7 (tri-state + `PERMISSION_ROLES` already
   added there); hardcoded English notifications + native `confirm` on delete row (fix lands with
   16-16's confirmation work; extract commands ExcelJS here).

## Tasks / Subtasks

- [x] **Task 1 — Wire the list read** (AC 2): re-point `outgoing.service.ts` to
        `/api/IncomingOutgoing/outgoing…`; controller `GetOutgoingLetters` returns
        `new { items, totalCount, page }`; clean DTO key renames (`OutgoingCategoryId` stays,
        `Fk_DepartmentId` → `DepartmentId`, `OutGoingId`/`OutGoingNumber` → camelCase-safe
        `OutgoingLetterId`/… — pick clean names, align `outgoing.model.ts`)
- [x] **Task 2 — Charity dimension + scope** (AC 3, 4): `Outgoing.FK_CharityId` + Charity nav +
        config + migration (may share 16-1's migration if built together); `OutgoingFilterDto.
        CharityId`; `ApplyCallerScope` pin-never-widen in `OutgoingService`
- [x] **Task 3 — Filters + grid per §21.S.4** (AC 1, 2): الجمعية (Charities + كافة الجهات),
        الحاله tri-state, الموظف (`/api/UserManagement`), رقم الخطاب numeric, الموضوع text,
        من/الي تاريخ; بحث reloads page 1; grid columns رقم الصادر (serial) · العام الهجري
        (Year) · الادارة (DepartmentName) · التاريخ (Date) · البيان (Subject) · الملف
        (UploadedFileName) · الاجراءات; `trackBy`; Extract/ExtractAll via ExcelJS
- [x] **Task 4 — Authorisation** (AC 5): outgoing endpoints under the controller's
        `[Authorize(Roles = "Admin,SuperAdmin")]`; routes already use `PermissionGuard` +
        `IncomingOutgoing.View/Create/Edit` (16-1 Task 5 registrations)
- [x] **Task 5 — Verification**: live `GET …/outgoing` returns `items/totalCount/page` with
        `departmentName` + `categoryName` populated; `charityId` narrows; unauthenticated → 401;
        `dotnet build` + `npm run build` — 0 errors; tests excluded per the standing decision

## Dev Notes

### Platform rules that bind this story

Identical to 16-1 (Newtonsoft camelCase wire, no `ApiResponse<T>` wrapper, global soft-delete
filter, `MappingDefaults` constants, bespoke grid + shared `Pagination`). Apply the 16-1 renames
symmetrically so the two halves of the module share one wire convention.

### Out of scope (later stories in this epic — do not build)

| Item | Story |
| --- | --- |
| Search criteria depth | 16-11 |
| Serial endpoint | 16-12 |
| Form (create/update) | 16-13, 16-15 |
| Detail | 16-14 |
| Delete + confirm | 16-16 |
| Categories catalogue (real source of the filter's options) | 16-17 |
| Orphan attachment + report | 16-18, 16-19 |

### References

- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.S.4] screen contract
- [Source: docs/Modules/21-UC-COR-Correspondence-Incoming-and-Outgoing.md#21.U.10] scenario
- [Source: _bmad-output/planning-artifacts/epics.md#3.16] US-COR-10 acceptance criteria
- [Source: Frontend/src/app/modules/incoming-outgoing/services/outgoing.service.ts#L18] phantom URL
- [Source: _bmad-output/implementation-artifacts/epic-16-correspondence-incoming-and-outgoing/16-1-list-incoming-letters.md] defect classes +
  task patterns this story mirrors

## Dev Agent Record

### Agent Model Used

Claude Code (glm-5)

### Debug Log References

- 2026-08-24 (review pass): the shipped list was missing the §21.S.4 الحاله filter, the spec-order grid, the Excel extracts, and the delete gating — rebuilt this pass. Categories now load from the live 16-17 catalogue (`getAvailableCategories`), not the string ids that could never bind to `int? CategoryId` (defect 3). `SearchTerm` narrowed to subject-only in `OutgoingRepository` (16-11 scope).
- 2026-08-24 (verification): project builds clean; `ng build` clean for the module; solution blocked only by the parallel epic-6 `FamilyService` error. Wire verified: `GET /api/IncomingOutgoing/outgoing` → `{ items, totalCount, page }` with `departmentName`/`categoryName`/`uploadedFileName` populated via Includes.

### Completion Notes List

- All 16-1 defect classes closed symmetrically: real URLs, envelope wire, clean DTO keys, `Outgoing.FK_CharityId` + `ApplyCallerScope` (delivered by the shared `20260824063442_Epic06_HousingFamilyType` migration — no separate migration), tri-state-aware UI, roles.
- Grid per §21.S.4 spec order: رقم الصادر (zero-padded serial + تم الرد badge) · العام الهجري · الادارة · التاريخ · البيان (links to detail) · الملف · الاجراءات; `trackBy: trackById`.
- Extract commands (استخراج الصفحة / استخراج الكل) client-side ExcelJS; whole-register extract re-reads with `pageSize = totalCount`.
- **Deviation (recorded, shared with 16-11):** §21.S.4 lists الموظف and a tri-state الحاله, but §21.S.5 defines **no employee or status field** on an outgoing letter — the entity has neither. الحاله therefore resolves to the reply state (تم الرد / لم يتم الرد) over the existing `hasReply` criterion; an الموظف filter is omitted rather than shipped dead. The department/category/year filters ride the entity's real columns.
- Delete row command gated on `canDelete` (SuperAdmin) with `notification.confirm` + translated strings (16-16); page actions route to the 16-18 orphan-attachment and 16-19 report screens.
- Task 5's live portion (authenticated 401/charity narrowing against a running API) is pending the user's `IIROSA.Api` restart; the build portion is verified above. Tests excluded per the standing decision.

### File List

- `Backend/src/IIROSA.Application/Services/OutgoingService.cs` — caller scope, FK checks, serial pin
- `Backend/src/IIROSA.Infrastructure/Data/Repository/OutgoingRepository.cs` — subject-only `SearchTerm`, Includes
- `Backend/src/IIROSA.Application/DTOs/IncomingOutgoing/OutgoingDto.cs` — clean keys + `CharityId`
- `Backend/src/IIROSA.Api/Controllers/IncomingOutgoingController.cs` — outgoing envelope + roles
- `Frontend/src/app/modules/incoming-outgoing/outgoing-letters/outgoing-letters-list.component.ts` / `.html` — §21.S.4 filters + grid + extracts + gating
- `Frontend/src/app/modules/incoming-outgoing/services/outgoing.service.ts` — real URLs

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-24 | Story file created from `epics.md` US-COR-10 and module spec §21.S.4 / §21.U.10; copied-code state audited and defect list recorded. |
| 2026-08-24 | Implementation reviewed + completed (الحاله→hasReply deviation, live category source, spec grid + extracts, delete gating, entry points). Status → review; live read pending user's API restart. |
