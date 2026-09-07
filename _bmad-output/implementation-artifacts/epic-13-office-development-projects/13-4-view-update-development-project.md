# Story 13-4: View / update a development project

| Field | Value |
| --- | --- |
| Story key | `13-4-view-update-development-project` |
| Epic | EP-13 — Office Development Projects |
| Use case | UC-OFP-04 — تعديل المشروع التنموي |
| Priority / size | Should · 5 points |
| Specification | `docs/Modules/18-UC-OFP-Office-Development-Projects.md` (§18.S.4 screen, §18.U.4 scenario) |
| Route | `#/office-development-projects/:id/edit` |
| Endpoint | `GET /api/OfficeProjectManagement/{id}` · `PUT /api/OfficeProjectManagement/{id}` |
| Depends on | 13-3 (DTO shape, validators, UoW) |

## Status

done

## Story

As a General Director, I want to be able to view / update a development project تعديل المشروع
التنموي, so that a record that was entered wrongly or has changed can be corrected.

## Acceptance Criteria

1. Given a General Director with an active session in the module, when the actor invokes the
   function with valid input, then the stored record carries the new values; no other record is
   affected.
2. Given the request is accepted, when it is served, then it is handled by
   `GET /api/OfficeProjectManagement/{id}` and the response is rendered on the screen without a
   page reload.
3. Given a mandatory field listed in the screen field specification is empty, when the actor saves,
   then the save is refused and the offending field is flagged.
4. Given the save succeeds, when the actor returns to the list screen, then the record appears
   there with the values just entered.
5. Given the session has expired or the role is not permitted, when the function is invoked, then
   the request is rejected and the actor is routed back to the login screen.

**Definition of done:** the screen fields of §18.S are implemented with their mandatory flags and
lookups; the scenario of §18.U passes end to end; the role and charity scoping is enforced
server-side, not only in the menu.

## Tasks / Subtasks

- [x] **Task 1 — Load the detail with navigations** (AC 2 — names render null today)
  - [x] Add `GetByIdWithDetailsAsync` to `IOfficeProjectRepository` /
        `OfficeProjectRepository` reusing `IncludeNavigationProperties()`
  - [x] Service `GetProjectByIdAsync` uses it so `OfficeProjectType`/`Country`/`Region`/`Center`/
        `Charity` names are populated in the detail DTO
- [x] **Task 2 — Fix the detail DTO keys so edit-mode patching works** (AC 1)
  - [x] Covered by 13-3 Task 1's rename; this story's verification: opening
        `#/office-development-projects/:id/edit` patches type, country, region, center and charity
        controls from the GET response (today the `fK_*` keys miss and those five controls stay
        empty — live-verified)
- [x] **Task 3 — Wire the update validator** (AC 3)
  - [x] Covered by 13-3 Task 2; verification: PUT with an empty mandatory field returns a
        field-flagged refusal
- [x] **Task 4 — Remove the spec-less `IsFinished` write-block**
  - [x] Delete `if (project.IsFinished) throw` from update (and delete in 13-5) — §18.U.4 lets a
        General Director correct any record; completion status is a data field, not a lock, and
        nothing in UC-OFP makes it one
- [x] **Task 5 — Scope the read to the caller's country**
  - [x] `GetProjectByIdAsync` / update / delete reject or scope records outside the caller's
        pinned country (fail closed for pinned callers, open for unpinned HQ)
- [~] **Task 6 — Tests** — EXCLUDED FROM SCOPE by user decision (same standing decision as epic 3;
      no test project exists under `Backend/tests`)

## Dev Notes

### Findings from the review that produced these tasks (2026-08-19)

- **The edit screen is broken end to end today.** Live GET returns the detail with `fK_*` keys and
  `null` for every navigation name (type/country/region/center/charity) because the service's
  private `GetByIdAsync` calls the repository's `GetByIdAsync`, which does no `Include`. The form's
  patch step then looks for `officeProjectTypeId`/`countryId`/… and finds neither the key nor the
  value. Both defects are fixed together: rename + include.
- The `IsFinished` guard on update (and delete) throws for finished projects. §18.U.4 contains no
  such rule; it blocks the exact correction flow this story exists for, and mark-complete is not
  irreversible elsewhere in the module (UC-OFP does not treat it as a lock).
- Role notes: Gen. Director ≈ `SuperAdmin`, Staff ≈ `Admin` (permission matrix F/F/–/–/–). Update
  stays `Admin,SuperAdmin` server-side; the menu already matches.

## Dev Agent Record

### Implementation Plan

1. Repository: `GetByIdWithDetailsAsync` with includes.
2. Service: use it for detail; drop the `IsFinished` blocks; apply the caller's country scope on
   read/update/delete.
3. Verify edit-mode patching live after 13-3's rename lands.

### Debug Log

- 2026-08-19: the update path deliberately does **not** map `UpdateOfficeProjectDto` through
  AutoMapper — the service hand-patches non-null values so a partial PUT does not blank the
  fields the payload left out. The profile comment records this.

### Completion Notes

- **Detail read fixed end to end**: `GetProjectByIdAsync` now loads through
  `GetByIdWithDetailsAsync` (includes type/country/region/center/charity/attachments), and the
  DTO keys are the clean names from 13-3 — the detail screen and the edit-form patch step read
  the same keys the API sends. Live GET returns `officeProjectTypeId: 2 / خياطه`,
  `countryId: 18 / مصر`, `regionId: 2 / جنوب سيناء`, `centerId: 57` — all five navigation names
  populated (all were `null` before).
- **`IsFinished` lock removed** from update (and delete, 13-5): §18.U.04 makes completion a data
  field. Verified live — PUT on a completed record returns 200 and carries the correction.
- **Scope**: `IsWithinCallerScope` on the detail read returns `null` for records outside the
  caller's pinned country (indistinguishable from a missing id); update applies the same check
  before patching.
- Detail template reads real wire fields (`officeProjectTypeName`, `charityName`,
  `document_Attach[0].fileName`, `createdOn`, `updatedOn`, `updatedBy`); `canEdit()` always true
  per §18.U.04.
- Tests excluded by user decision.

## File List

| File | Change |
| --- | --- |
| `Backend/src/IIROSA.Domain/Interfaces/IOfficeProjectRepository.cs` | `GetByIdWithDetailsAsync` added; ~9 unused helpers removed |
| `Backend/src/IIROSA.Infrastructure/Data/Repository/OfficeProjectRepository.cs` | Implements `GetByIdWithDetailsAsync` via `IncludeNavigationProperties()` |
| `Backend/src/IIROSA.Application/Services/OfficeProjectService.cs` | Detail via includes; `IsWithinCallerScope`; `IsFinished` blocks removed; hand-patch update loop on clean names |
| `Backend/src/IIROSA.Application/Profiles/OfficeProjectProfile.cs` | Detail entity→DTO ForMembers for every FK id + name pair |
| `Frontend/src/app/modules/office-development-projects/project-detail/project-detail.component.html` | Reads real wire fields (7 bindings) |
| `Frontend/src/app/modules/office-development-projects/project-detail/project-detail.component.ts` | `canEdit()` always true (§18.U.04) |

## Change Log

| Date | Change |
| --- | --- |
| 2026-08-19 | Story file created from `epics.md` US-OFP-04 and module spec §18.S.4 / §18.U.4; audit findings recorded. |
| 2026-08-19 | Tasks 1–5 implemented; detail names + edit patching + update-after-complete verified live; backend 0 errors, frontend builds. |
